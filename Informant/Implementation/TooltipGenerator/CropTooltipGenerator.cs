using Microsoft.Xna.Framework;
using Slothsoft.Informant.Api;
using Slothsoft.Informant.Implementation.Common;
using StardewValley.TerrainFeatures;
using StardewValley.TokenizableStrings;

namespace Slothsoft.Informant.Implementation.TooltipGenerator;

internal class CropTooltipGenerator : ITooltipGenerator<TerrainFeature>
{
    private readonly IModHelper _modHelper;

    public CropTooltipGenerator(IModHelper modHelper)
    {
        _modHelper = modHelper;
    }

    public string Id => "crop";
    public string DisplayName => _modHelper.Translation.Get("CropTooltipGenerator");
    public string Description => _modHelper.Translation.Get("CropTooltipGenerator.Description");

    public bool HasTooltip(TerrainFeature input)
    {
        return input is HoeDirt { crop: { } } && IsInContext(input.Location);
    }

    public Tooltip Generate(TerrainFeature input)
    {
        return CreateTooltip(_modHelper, (HoeDirt)input);
    }

    internal static bool IsInContext(GameLocation location)
    {
        return location.IsFarm || location.IsGreenhouse || location.InIslandContext();
    }

    internal static Tooltip CreateTooltip(IModHelper modHelper, HoeDirt dirt)
    {
        var crop = dirt.crop;
        // for some reason, ginger is displayed as weeds
        var cropId = crop.whichForageCrop.Value == Crop.forageCrop_gingerID ? CropIds.Ginger : crop.indexOfHarvest.Value;
        var produce = ItemRegistry.GetDataOrErrorItem(cropId);
        var displayName = TokenParser.ParseText(produce.DisplayName);
        var daysLeft = CalculateDaysLeftString(modHelper, crop);

        bool hasCrop = dirt.crop is not null;
        bool cropDead = crop.dead.Value;
        bool notWatered = dirt.state.Value != 1 && hasCrop && !cropDead;
        bool hasFertilizer = dirt.HasFertilizer();

        string tooltipText = $"{displayName}\n{daysLeft}";
        // NotWatered text
        if (notWatered) {
            var notWateredText = modHelper.Translation.Get("CropTooltipGenerator.NotWatered");
            tooltipText += $"\n{notWateredText}";
        }

        if (hasFertilizer && dirt.fertilizer.Value.Contains('|')) {
            tooltipText += "\n\n";
        }

        return new Tooltip(tooltipText) {
            Icon = [
                hasCrop ?
                Icon.ForUnqualifiedItemId(
                    // dead crop icon
                    !cropDead ? cropId : "748",
                    IPosition.CenterRight,
                    new Vector2(Game1.tileSize / 2f, Game1.tileSize / 2f)
                ) : null,
                // Adds compatibility to fertilizer mods
                .. GetFertilizerIconList(dirt),
                // Not watered soil
                notWatered ?
                Icon.ForUnqualifiedItemId(
                    GetWateringCanId(),
                    IPosition.CenterRight,
                    new Vector2(Game1.tileSize / 2f, Game1.tileSize / 2f)
                ) : null
            ]
        };
    }

    internal static string CalculateDaysLeftString(IModHelper modHelper, Crop crop)
    {
        if (crop.dead.Value) {
            return modHelper.Translation.Get("CropTooltipGenerator.Dead");
        }
        return ToDaysLeftString(modHelper, CalculateDaysLeft(crop));
    }

    internal static string ToDaysLeftString(IModHelper modHelper, int daysLeft, bool bush = false)
    {
        return daysLeft switch {
            -1 => "", // something went very wrong, but we don't want to break the game
            0 => modHelper.Translation.Get("CropTooltipGenerator.0DaysLeft"),
            1 => modHelper.Translation.Get(bush ? "CropTooltipGenerator.1DayLeftMature" : "CropTooltipGenerator.1DayLeft"),
            _ => modHelper.Translation.Get(bush ? "CropTooltipGenerator.XDaysLeftMature" : "CropTooltipGenerator.XDaysLeft", new { X = daysLeft })
        };
    }

    internal static int CalculateDaysLeft(Crop crop)
    {
        var currentPhase = crop.currentPhase.Value;
        var dayOfCurrentPhase = crop.dayOfCurrentPhase.Value;
        var regrowAfterHarvest = crop.RegrowsAfterHarvest();
        var cropPhaseDays = crop.phaseDays.ToArray();

        // Amaranth:  current = 4 | day = 0 | days = 1, 2, 2, 2, 99999 | result => 0
        // Fairy Rose:  current = 4 | day = 1 | days = 1, 4, 4, 3, 99999 | result => 0
        // Cranberry:  current = 5 | day = 4 | days = 1, 2, 1, 1, 2, 99999 | result => ???
        // Ancient Fruit: current = 5 | day = 4 | days = 1 5 5 6 4 99999 | result => 4
        // Blueberry (harvested): current = 5 | day = 4 | days = 1 3 3 4 2 99999 | regrowAfterHarvest = 4 | result => 4
        // Blueberry (harvested): current = 5 | day = 0 | days = 1 3 3 4 2 99999 | regrowAfterHarvest = 4 | result => 0
        var result = 0;
        for (var phase = currentPhase; phase < cropPhaseDays.Length; phase++) {
            if (cropPhaseDays[phase] < 99999) {
                result += cropPhaseDays[phase];
                if (phase == currentPhase) {
                    result -= dayOfCurrentPhase;
                }
            } else if (currentPhase == cropPhaseDays.Length - 1 && regrowAfterHarvest) {
                // calculate the repeating harvests, it seems the dayOfCurrentPhase counts backwards now
                result = dayOfCurrentPhase;
            }
        }

        return result;
    }
    // Upgrades icon with farming level
    internal static string GetWateringCanId() {
        int level = Game1.player.farmingLevel.Value;
        string quality = level switch {
            > 8 => "Iridium",
            > 5 => "Gold",
            > 4 => "Steel",
            > 2 => "Copper",
            _ => "",
        };
        return $"(T){quality}WateringCan";
    }

    // Adds compatibility to fertilizer mods
    internal static Icon?[] GetFertilizerIconList(HoeDirt dirt) {
        if ((InformantMod.Instance?.Config.DecorateNotWatered ?? false) && dirt.HasFertilizer()) {
            if (dirt.fertilizer.Value.Contains('|')) {
                return [.. dirt.fertilizer.Value.Split("|")
                    .GroupBy(id => ItemRegistry.GetData(id)?.DisplayName ?? "Unknown Fertilizer")
                    .OrderBy(g => g.Key)
                    .SelectMany(g => g)
                    .Select(f =>
                        Icon.ForUnqualifiedItemId(
                            f,
                            IPosition.BottomCenter,
                            new Vector2(Game1.tileSize / 2f, Game1.tileSize / 2f)
                        )
                    )];
            } else {
                return [Icon.ForUnqualifiedItemId(
                    dirt.fertilizer.Value,
                    IPosition.CenterRight,
                    new Vector2(Game1.tileSize / 2f, Game1.tileSize / 2f)
                )];
            };
        }
        return [];
    }
}