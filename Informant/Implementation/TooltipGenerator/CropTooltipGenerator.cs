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
        return input is HoeDirt { crop: { } };
    }

    public Tooltip Generate(TerrainFeature input)
    {
        return CreateTooltip(_modHelper, ((HoeDirt)input));
    }

    internal static Tooltip CreateTooltip(IModHelper modHelper, HoeDirt dirt)
    {
        var crop = dirt.crop;
        // for some reason, ginger is displayed as weeds
        var parentSheetIndexString = crop.whichForageCrop.Value == Crop.forageCrop_gingerID ? ObjectIds.Ginger : crop.indexOfHarvest.Value;
        Game1.objectData.TryGetValue(parentSheetIndexString, out var data);
        var displayName = TokenParser.ParseText(data?.DisplayName) ?? ItemRegistry.GetErrorItemName();
        var daysLeft = CalculateDaysLeftString(modHelper, crop);

        return new Tooltip($"{displayName}\n{daysLeft}") {
            Icon =
            [
                Icon.ForParentSheetIndex(
                    parentSheetIndexString,
                    IPosition.CenterRight,
                    new Vector2(Game1.tileSize / 2f, Game1.tileSize / 2f)
                ),
                dirt.HasFertilizer() && (InformantMod.Instance?.Config.ShowCropFertilizer ?? false) ?
                Icon.ForParentSheetIndex(
                    dirt.fertilizer.Value,
                    IPosition.CenterRight,
                    new Vector2(Game1.tileSize / 2f, Game1.tileSize / 2f)
                ) : null,
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

    internal static string ToDaysLeftString(IModHelper modHelper, int daysLeft)
    {
        return daysLeft switch {
            -1 => "", // something went very wrong, but we don't want to break the game
            0 => modHelper.Translation.Get("CropTooltipGenerator.0DaysLeft"),
            1 => modHelper.Translation.Get("CropTooltipGenerator.1DayLeft"),
            _ => modHelper.Translation.Get("CropTooltipGenerator.XDaysLeft", new { X = daysLeft })
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
}