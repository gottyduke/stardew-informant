using Informant.ThirdParty.CustomBush;
using Microsoft.Xna.Framework;
using Slothsoft.Informant.Api;
using StardewValley.TerrainFeatures;
using Informant.ThirdParty;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using System.Diagnostics.CodeAnalysis;
using StardewModdingAPI.Utilities;

namespace Slothsoft.Informant.Implementation.TooltipGenerator;

internal class TeaBushTooltipGenerator : ITooltipGenerator<TerrainFeature>
{

    private readonly IModHelper _modHelper;
    private readonly IEnumerable<int> _bloomWeek = Enumerable.Range(22, 7);

    public TeaBushTooltipGenerator(IModHelper modHelper)
    {
        _modHelper = modHelper;
    }

    public string Id => "tea-bush";
    public string DisplayName => _modHelper.Translation.Get("TeaBushTooltipGenerator");
    public string Description => _modHelper.Translation.Get("TeaBushTooltipGenerator.Description");

    public string NotThisSeason => _modHelper.Translation.Get("CustomBushTooltipGenerator.NotThisSeason");

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
        if (HookToCustomBush.GetApi(out ICustomBushApi? customBushApi))
        {
            if (customBushApi.TryGetCustomBush(bush, out ICustomBush? customBushData, out string? id))
            {
                displayName = customBushData.DisplayName;
                willProduceThisSeason = customBushData.Seasons.Contains(Game1.season);
                ageToMature = customBushData.AgeToProduce;

                // Handle custom drops
                if (customBushApi.TryGetDrops(id, out IList<ICustomBushDrop>? drops) &&
                    drops != null && drops.Count > 0)
                {
                    item = ItemRegistry.GetDataOrErrorItem(drops[0].ItemId);
                }

                daysLeft = CalculateCustomBushDaysLeft(bush, customBushData, id);
            }
        }

        // Construct tooltip text
        // Determine if the bush is still maturing
        bool isMaturing = bush.getAge() < ageToMature;
        var daysLeftText = CropTooltipGenerator.ToDaysLeftString(_modHelper, daysLeft, isMaturing);

        // Construct tooltip text
        string tooltipText = displayName;

        if (isMaturing)
        {
            // Bush is still growing; show days until maturity
            tooltipText += $"\n{daysLeftText}";

            // If it's not in season, show additional note
            if (!willProduceThisSeason)
            {
                tooltipText += $"\n{NotThisSeason}";
            }
        }
        else
        {
            // Bush is mature; show days until the next production or Out of season
            tooltipText += $"\n{(willProduceThisSeason ? daysLeftText : NotThisSeason)}";
        }

        return new Tooltip(tooltipText)
        {
            Icon = [Icon.ForUnqualifiedItemId(
            item.QualifiedItemId,
            IPosition.CenterRight,
            new Vector2(Game1.tileSize / 2, Game1.tileSize / 2)
        )]
        };
    }

    /// <summary>
    /// The Tea Sapling is a seed that takes 20 days to grow into a Tea Bush. 
    /// A Tea Bush produces one Tea Leaves item each day of the final week (days 22-28) of 
    /// spring, summer, and fall (and winter if indoors).
    /// </summary>
    internal int CalculateDaysLeft(Bush bush)
    {
        if (bush.tileSheetOffset.Value == 1)
        {
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

        if (daysLeft < 0)
        {
            // fully grown
            daysLeft += WorldDate.DaysPerMonth;
        }

        int nextSeason = (daysLeft + today) / WorldDate.DaysPerMonth;
        // outdoor tea bush cannot shake in winter
        if (!bush.IsSheltered() && Game1.Date.SeasonIndex + nextSeason == (int)Season.Winter)
        {
            daysLeft += WorldDate.DaysPerMonth;
        }

        return daysLeft;
    }

    internal static int CalculateCustomBushDaysLeft(Bush bush, ICustomBush customBushData, string id)
    {
        var today = SDate.Now();
        var nextHarvestDate = GetNextHarvestDate(bush, customBushData);

        // If bush isn't mature yet, calculate days until maturity
        var bushAge = bush.getAge();
        if (bushAge < customBushData.AgeToProduce)
        {
            return Math.Max(0, customBushData.AgeToProduce - bushAge + 1);
        }

        // For mature bushes, return days until next harvest
        int daysUntilHarvest = GetDaysBetween(today, nextHarvestDate);
        return Math.Max(0, daysUntilHarvest);
    }

    internal static SDate GetNextHarvestDate(Bush bush, ICustomBush customBush)
    {
        SDate today = SDate.Now();
        var tomorrow = today.AddDays(1);

        // currently has produce
        if (bush.tileSheetOffset.Value == 1)
            return today;

        // tea bush and custom bush
        int dayToBegin = customBush.DayToBeginProducing;
        if (dayToBegin >= 0)
        {
            SDate readyDate = GetDateFullyGrown(bush, customBush);
            if (readyDate < tomorrow)
                readyDate = tomorrow;

            if (!bush.IsSheltered())
            {
                // bush not sheltered, must check producing seasons
                List<Season> producingSeasons = customBush.Seasons;
                SDate seasonDate = new(Math.Max(1, dayToBegin), readyDate.Season, readyDate.Year);
                while (!producingSeasons.Contains(seasonDate.Season))
                    seasonDate = seasonDate.AddDays(28);

                if (readyDate < seasonDate)
                    return seasonDate;
            }

            if (readyDate.Day < dayToBegin)
                readyDate = new(dayToBegin, readyDate.Season, readyDate.Year);

            return readyDate;
        }

        // wild bushes produce salmonberries in spring 15-18, and blackberries in fall 8-11
        SDate springStart = new(15, Season.Spring);
        SDate springEnd = new(18, Season.Spring);
        SDate fallStart = new(8, Season.Fall);
        SDate fallEnd = new(11, Season.Fall);

        if (tomorrow < springStart)
            return springStart;
        if (tomorrow > springEnd && tomorrow < fallStart)
            return fallStart;
        if (tomorrow > fallEnd)
            return new(springStart.Day, springStart.Season, springStart.Year + 1);
        return tomorrow;
    }

    internal static SDate GetDateFullyGrown(Bush bush, ICustomBush customBush)
    {
        SDate date = new(1, Season.Spring, 1);
        date = date.AddDays(bush.datePlanted.Value);
        date = date.AddDays(customBush.AgeToProduce);

        return date;
    }

    internal static int GetDaysBetween(SDate start, SDate end)
    {
        // Convert both dates to total days since game start
        int startDays = start.DaysSinceStart;
        int endDays = end.DaysSinceStart;

        return endDays - startDays;
    }
}