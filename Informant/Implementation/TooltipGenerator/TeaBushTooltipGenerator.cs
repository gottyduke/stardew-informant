using Informant.ThirdParty;
using Informant.ThirdParty.CustomBush;
using Microsoft.Xna.Framework;
using Slothsoft.Informant.Api;
using StardewValley.TerrainFeatures;
using StardewValley.TokenizableStrings;

namespace Slothsoft.Informant.Implementation.TooltipGenerator;

internal class TeaBushTooltipGenerator : ITooltipGenerator<TerrainFeature>
{
    private readonly IEnumerable<int> _bloomWeek = Enumerable.Range(22, 7);

    private readonly IModHelper _modHelper;

    public TeaBushTooltipGenerator(IModHelper modHelper)
    {
        _modHelper = modHelper;
    }

    public string NotThisSeason => _modHelper.Translation.Get("CustomBushTooltipGenerator.NotThisSeason");
    public string NotThisSeasonAnymore => _modHelper.Translation.Get("CustomBushToolTipGenerator.NotThisSeasonAnymore");

    public string Id => "tea-bush";
    public string DisplayName => _modHelper.Translation.Get("TeaBushTooltipGenerator");
    public string Description => _modHelper.Translation.Get("TeaBushTooltipGenerator.Description");

    public bool HasTooltip(TerrainFeature input)
    {
        return input is Bush bush && !bush.townBush.Value && bush.size.Value == Bush.greenTeaBush;
    }

    public Tooltip Generate(TerrainFeature input)
    {
        return CreateTooltip((Bush)input);
    }

    private Tooltip CreateTooltip(Bush bush)
    {
        // Default values for regular tea bush
        var item = ItemRegistry.GetDataOrErrorItem(bush.GetShakeOffItem());
        var displayName = item.DisplayName;
        var daysLeft = CalculateDaysLeft(bush);
        var ageToMature = Bush.daysToMatureGreenTeaBush;
        var willProduceThisSeason = true;

        // Handle custom bush logic
        if (HookToCustomBush.GetApi(out ICustomBushApi? customBushApi)) {
            if (customBushApi.TryGetCustomBush(bush, out var customBushData, out var id)) {
                displayName = customBushData.DisplayName;
                if (displayName.Contains("LocalizedText")) {
                    displayName = TokenParser.ParseText(displayName);
                }

                willProduceThisSeason = customBushData.Seasons.Contains(Game1.season);
                ageToMature = customBushData.AgeToProduce;

                // Handle custom drops
                if (customBushApi.TryGetDrops(id, out var drops) && drops.Count > 0) {
                    item = ItemRegistry.GetDataOrErrorItem(drops[0].ItemId);
                }

                daysLeft = CalculateCustomBushDaysLeft(bush, customBushData, id, customBushApi);
            }
        }

        // Construct tooltip text
        // Determine if the bush is still maturing
        var isMaturing = bush.getAge() < ageToMature;
        var daysLeftText = CropTooltipGenerator.ToDaysLeftString(_modHelper, daysLeft, isMaturing);

        // Construct tooltip text
        var tooltipText = displayName;

        if (isMaturing) {
            // Bush is still growing; show days until maturity
            tooltipText += $"\n{daysLeftText}";

            // If it's not in season, show additional note
            if (!willProduceThisSeason) {
                tooltipText += $"\n{NotThisSeason}";
            }
        } else {
            // Bush is mature; show days until the next production or Out of season
            tooltipText += $"\n{(willProduceThisSeason ? daysLeft == -1 ? NotThisSeasonAnymore : daysLeftText : NotThisSeason)}";
        }

        return new(tooltipText) {
            Icon = [
                Icon.ForUnqualifiedItemId(
                    item.QualifiedItemId,
                    IPosition.CenterRight,
                    new Vector2(Game1.tileSize / 2, Game1.tileSize / 2)
                ),
            ],
        };
    }

    /// <summary>
    ///     The Tea Sapling is a seed that takes 20 days to grow into a Tea Bush.
    ///     A Tea Bush produces one Tea Leaves item each day of the final week (days 22-28) of
    ///     spring, summer, and fall (and winter if indoors).
    /// </summary>
    internal int CalculateDaysLeft(Bush bush)
    {
        if (bush.tileSheetOffset.Value == 1) {
            // has tea leaves
            return 0;
        }

        var today = Game1.Date.DayOfMonth;
        var futureDay = Bush.daysToMatureGreenTeaBush + bush.datePlanted.Value;
        var daysLeft = futureDay - Game1.Date.TotalDays - 1;

        daysLeft = daysLeft <= 0 ? 0 : daysLeft;
        var bloomDay = (daysLeft + today) % WorldDate.DaysPerMonth;
        // add up the next closest bloom day
        daysLeft += _bloomWeek.Contains(bloomDay) ? 1 : _bloomWeek.First() - bloomDay;

        if (daysLeft < 0) {
            // fully grown
            daysLeft += WorldDate.DaysPerMonth;
        }

        var nextSeason = (daysLeft + today) / WorldDate.DaysPerMonth;
        // outdoor tea bush cannot shake in winter
        if (!bush.IsSheltered() && Game1.Date.SeasonIndex + nextSeason == (int)Season.Winter) {
            daysLeft += WorldDate.DaysPerMonth;
        }

        return daysLeft;
    }

    internal static int CalculateCustomBushDaysLeft(Bush bush, ICustomBush customBushData, string id,
        ICustomBushApi customBushApi)
    {
        // If not mature yet, calculate days until maturity
        var bushAge = bush.getAge();
        if (bushAge < customBushData.AgeToProduce) {
            return Math.Max(0, customBushData.AgeToProduce - bushAge);
        }

        // If already has items ready
        if (bush.tileSheetOffset.Value == 1) {
            return 0;
        }

        // If in production period and ready
        if (customBushData.GetShakeOffItemIfReady(bush, out var shakeOffItemData)) {
            var item = new CustomBushExtensions.PossibleDroppedItem(Game1.dayOfMonth, shakeOffItemData, 1.0f, id);
            if (item.ReadyToPick) {
                return 0;
            }
        } else {
            // Get the list of possible drops to check production schedule
            var drops = customBushApi.GetCustomBushDropItems(customBushData, id);
            if (drops.Any()) {
                // Find the next production day from the drops
                var nextProductionDay = drops
                    .Select(drop => drop.NextDayToProduce)
                    .Where(day => day > Game1.dayOfMonth)
                    .DefaultIfEmpty(customBushData.DayToBeginProducing +
                                    WorldDate.DaysPerMonth) // If no days found, use next month
                    .Min();

                return nextProductionDay - Game1.dayOfMonth;
            }
        }

        // If no production schedule found but in production period,
        // check if it's a valid production day
        var inProductionPeriod = Game1.dayOfMonth >= customBushData.DayToBeginProducing;
        if (!inProductionPeriod) {
            return Math.Max(0, customBushData.DayToBeginProducing - Game1.dayOfMonth);
        }

        // Check if production conditions are met (season, location, etc)
        if (!customBushData.Seasons.Contains(Game1.season) ||
            !bush.IsSheltered()) {
            // Cannot produce under current conditions, try next season
            return -1;
        }

        // Not yet in production period
        return Math.Max(0, customBushData.DayToBeginProducing - Game1.dayOfMonth);
    }
}