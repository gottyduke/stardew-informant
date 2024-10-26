using Informant.ThirdParty.CustomBush;
using Microsoft.Xna.Framework;
using Slothsoft.Informant.Api;
using StardewValley.TerrainFeatures;
using Informant.ThirdParty;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using System.Diagnostics.CodeAnalysis;
using StardewModdingAPI.Utilities;
using StardewValley.Extensions;

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

                daysLeft = CalculateCustomBushDaysLeft(bush, customBushData, id, customBushApi);
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

    internal static int CalculateCustomBushDaysLeft(Bush bush, ICustomBush customBushData, string id, ICustomBushApi customBushApi)
    {
        // If not mature yet, calculate days until maturity
        var bushAge = bush.getAge();
        if (bushAge < customBushData.AgeToProduce)
        {
            return Math.Max(0, customBushData.AgeToProduce - bushAge + 1);
        }

        // If already has items ready
        if (bush.tileSheetOffset.Value == 1)
        {
            return 0;
        }

        // If in production period and ready
        if (GetShakeOffItemIfReady(customBushData, bush, out ParsedItemData? shakeOffItemData))
        {
            var item = new PossibleDroppedItem(Game1.dayOfMonth, shakeOffItemData, 1.0f, id);
            if (item.ReadyToPick) return 0;
        }
        else
        {
            // Get the list of possible drops to check production schedule
            var drops = GetCustomBushDropItems(customBushApi, customBushData, id);
            if (drops.Any())
            {
                // Find the next production day from the drops
                var nextProductionDay = drops
                    .Select(drop => drop.NextDayToProduce)
                    .Where(day => day > Game1.dayOfMonth)
                    .DefaultIfEmpty(customBushData.DayToBeginProducing + WorldDate.DaysPerMonth) // If no days found, use next month
                    .Min();

                return nextProductionDay - Game1.dayOfMonth;
            }
        }

        // If no production schedule found but in production period,
        // check if it's a valid production day
        bool inProductionPeriod = Game1.dayOfMonth >= customBushData.DayToBeginProducing;
        if (inProductionPeriod)
        {
            // Check if production conditions are met (season, location, etc)
            if (!customBushData.Seasons.Contains(Game1.season) ||
                (bush.IsSheltered()) ||
                (!bush.IsSheltered()))
            {
                // Cannot produce under current conditions, try next season
                return WorldDate.DaysPerMonth - Game1.dayOfMonth + customBushData.DayToBeginProducing;
            }
        }

        // Not yet in production period
        return Math.Max(0, customBushData.DayToBeginProducing - Game1.dayOfMonth);
    }

    internal static bool GetShakeOffItemIfReady(
    ICustomBush customBush,
    Bush bush,
    [NotNullWhen(true)] out ParsedItemData? item
  )
    {
        item = null;
        if (bush.size.Value != Bush.greenTeaBush)
        {
            return false;
        }

        if (!bush.modData.TryGetValue("furyx639.CustomBush/ShakeOff", out string itemId))
        {
            return false;
        }

        item = ItemRegistry.GetData(itemId);
        return true;
    }

    internal static List<PossibleDroppedItem> GetCustomBushDropItems(
        ICustomBushApi api,
        ICustomBush bush,
        string? id,
        bool includeToday = false
      )
    {
        if (id == null || string.IsNullOrEmpty(id))
        {
            return new List<PossibleDroppedItem>();
        }

        api.TryGetDrops(id, out IList<ICustomBushDrop>? drops);
        return drops == null
          ? new List<PossibleDroppedItem>()
          : GetGenericDropItems(drops, id, includeToday, bush.DisplayName, BushDropConverter);

        DropInfo BushDropConverter(ICustomBushDrop input)
        {
            return new DropInfo(input.Condition, input.Chance, input.ItemId);
        }
    }

    public static List<PossibleDroppedItem> GetGenericDropItems<T>(
    IEnumerable<T> drops,
    string? customId,
    bool includeToday,
    string displayName,
    Func<T, DropInfo> extractDropInfo
  )
    {
        List<PossibleDroppedItem> items = new();

        foreach (T drop in drops)
        {
            DropInfo dropInfo = extractDropInfo(drop);
            int? nextDay = GetNextDay(dropInfo.Condition, includeToday);
            int? lastDay = GetLastDay(dropInfo.Condition);

            if (!nextDay.HasValue)
            {
                if (!lastDay.HasValue)
                {
                }

                continue;
            }

            ParsedItemData? itemData = ItemRegistry.GetData(dropInfo.ItemId);
            if (itemData == null)
            {
                continue;
            }

            if (Game1.dayOfMonth == nextDay.Value && !includeToday)
            {
                continue;
            }

            items.Add(new PossibleDroppedItem(nextDay.Value, itemData, dropInfo.Chance, customId));
        }

        return items;
    }

    public static int? GetNextDay(string? condition, bool includeToday)
    {
        return string.IsNullOrEmpty(condition)
          ? Game1.dayOfMonth + (includeToday ? 0 : 1)
          : GetNextDayFromCondition(condition, includeToday);
    }

    public static int? GetLastDay(string? condition)
    {
        return GetLastDayFromCondition(condition);
    }

    public record PossibleDroppedItem(int NextDayToProduce, ParsedItemData Item, float Chance, string? CustomId = null)
    {
        public bool ReadyToPick => Game1.dayOfMonth == NextDayToProduce;
    }

    public record DropInfo(string? Condition, float Chance, string ItemId)
    {
        public int? GetNextDay(bool includeToday)
        {
            return LocalGetNextDay(Condition, includeToday);
        }
    }

    public static int? LocalGetNextDay(string? condition, bool includeToday)
    {
        return string.IsNullOrEmpty(condition)
          ? Game1.dayOfMonth + (includeToday ? 0 : 1)
          : GetNextDayFromCondition(condition, includeToday);
    }

    public static int? GetNextDayFromCondition(string? condition, bool includeToday = true)
    {
        HashSet<int> days = new();
        if (condition == null)
        {
            return null;
        }

        GameStateQuery.ParsedGameStateQuery[]? conditionEntries = GameStateQuery.Parse(condition);

        foreach (GameStateQuery.ParsedGameStateQuery parsedGameStateQuery in conditionEntries)
        {
            days.AddRange(GetDaysFromCondition(parsedGameStateQuery));
        }

        days.RemoveWhere(day => day < Game1.dayOfMonth || (!includeToday && day == Game1.dayOfMonth));

        return days.Count == 0 ? null : days.Min();
    }

    public static IEnumerable<int> GetDaysFromCondition(GameStateQuery.ParsedGameStateQuery parsedGameStateQuery)
    {
        HashSet<int> days = new();
        if (parsedGameStateQuery.Query.Length < 2)
        {
            return days;
        }

        string queryStr = parsedGameStateQuery.Query[0];
        if (!"day_of_month".Equals(queryStr, StringComparison.OrdinalIgnoreCase))
        {
            return days;
        }

        for (var i = 1; i < parsedGameStateQuery.Query.Length; i++)
        {
            string dayStr = parsedGameStateQuery.Query[i];
            if ("even".Equals(dayStr, StringComparison.OrdinalIgnoreCase))
            {
                days.AddRange(Enumerable.Range(1, 28).Where(x => x % 2 == 0));
                continue;
            }

            if ("odd".Equals(dayStr, StringComparison.OrdinalIgnoreCase))
            {
                days.AddRange(Enumerable.Range(1, 28).Where(x => x % 2 != 0));
                continue;
            }

            try
            {
                int parsedInt = int.Parse(dayStr);
                days.Add(parsedInt);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        return parsedGameStateQuery.Negated ? Enumerable.Range(1, 28).Where(x => !days.Contains(x)).ToHashSet() : days;
    }

    public static int? GetLastDayFromCondition(string? condition)
    {
        HashSet<int> days = new();
        if (condition == null)
        {
            return null;
        }

        GameStateQuery.ParsedGameStateQuery[]? conditionEntries = GameStateQuery.Parse(condition);

        foreach (GameStateQuery.ParsedGameStateQuery parsedGameStateQuery in conditionEntries)
        {
            days.AddRange(GetDaysFromCondition(parsedGameStateQuery));
        }

        return days.Count == 0 ? null : days.Max();
    }
}