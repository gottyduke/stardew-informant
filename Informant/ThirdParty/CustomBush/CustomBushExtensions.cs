using StardewValley.Extensions;
using StardewValley.ItemTypeDefinitions;
using StardewValley.TerrainFeatures;
using System.Diagnostics.CodeAnalysis;

namespace Informant.ThirdParty.CustomBush;
internal static class CustomBushExtensions
{
    private const string ShakeOffItem = "furyx639.CustomBush/ShakeOff";

    public static bool GetShakeOffItemIfReady(
        this ICustomBush customBush,
        Bush bush,
        [NotNullWhen(true)] out ParsedItemData? item
      )
    {
        item = null;
        if (bush.size.Value != Bush.greenTeaBush) {
            return false;
        }

        if (!bush.modData.TryGetValue(ShakeOffItem, out string itemId)) {
            return false;
        }

        item = ItemRegistry.GetData(itemId);
        return true;
    }

    public static List<PossibleDroppedItem> GetCustomBushDropItems(
        this ICustomBushApi api,
        ICustomBush bush,
        string? id,
        bool includeToday = false
      )
    {
        if (id == null || string.IsNullOrEmpty(id)) {
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

        foreach (T drop in drops) {
            DropInfo dropInfo = extractDropInfo(drop);
            int? nextDay = GetNextDay(dropInfo.Condition, includeToday);
            int? lastDay = GetLastDay(dropInfo.Condition);

            if (!nextDay.HasValue) {
                if (!lastDay.HasValue) {
                }

                continue;
            }

            ParsedItemData? itemData = ItemRegistry.GetData(dropInfo.ItemId);
            if (itemData == null) {
                continue;
            }

            if (Game1.dayOfMonth == nextDay.Value && !includeToday) {
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
        if (condition == null) {
            return null;
        }

        GameStateQuery.ParsedGameStateQuery[]? conditionEntries = GameStateQuery.Parse(condition);

        foreach (GameStateQuery.ParsedGameStateQuery parsedGameStateQuery in conditionEntries) {
            days.AddRange(GetDaysFromCondition(parsedGameStateQuery));
        }

        days.RemoveWhere(day => day < Game1.dayOfMonth || (!includeToday && day == Game1.dayOfMonth));

        return days.Count == 0 ? null : days.Min();
    }

    public static IEnumerable<int> GetDaysFromCondition(GameStateQuery.ParsedGameStateQuery parsedGameStateQuery)
    {
        HashSet<int> days = new();
        if (parsedGameStateQuery.Query.Length < 2) {
            return days;
        }

        string queryStr = parsedGameStateQuery.Query[0];
        if (!"day_of_month".Equals(queryStr, StringComparison.OrdinalIgnoreCase)) {
            return days;
        }

        for (var i = 1; i < parsedGameStateQuery.Query.Length; i++) {
            string dayStr = parsedGameStateQuery.Query[i];
            if ("even".Equals(dayStr, StringComparison.OrdinalIgnoreCase)) {
                days.AddRange(Enumerable.Range(1, 28).Where(x => x % 2 == 0));
                continue;
            }

            if ("odd".Equals(dayStr, StringComparison.OrdinalIgnoreCase)) {
                days.AddRange(Enumerable.Range(1, 28).Where(x => x % 2 != 0));
                continue;
            }

            try {
                int parsedInt = int.Parse(dayStr);
                days.Add(parsedInt);
            } catch (Exception) {
                // ignored
            }
        }

        return parsedGameStateQuery.Negated ? Enumerable.Range(1, 28).Where(x => !days.Contains(x)).ToHashSet() : days;
    }

    public static int? GetLastDayFromCondition(string? condition)
    {
        HashSet<int> days = new();
        if (condition == null) {
            return null;
        }

        GameStateQuery.ParsedGameStateQuery[]? conditionEntries = GameStateQuery.Parse(condition);

        foreach (GameStateQuery.ParsedGameStateQuery parsedGameStateQuery in conditionEntries) {
            days.AddRange(GetDaysFromCondition(parsedGameStateQuery));
        }

        return days.Count == 0 ? null : days.Max();
    }
}
