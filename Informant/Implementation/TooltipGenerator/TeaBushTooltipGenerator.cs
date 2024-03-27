using Microsoft.Xna.Framework;
using Slothsoft.Informant.Api;
using StardewValley.TerrainFeatures;

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
        var item = ItemRegistry.GetDataOrErrorItem(bush.GetShakeOffItem());
        var displayName = item.DisplayName;
        var daysLeft = CropTooltipGenerator.ToDaysLeftString(_modHelper, CalculateDaysLeft(bush));
        return new Tooltip($"{displayName}\n{daysLeft}") {
            Icon = [Icon.ForParentSheetIndex(
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

        int nextSeason = (daysLeft + today) / WorldDate.DaysPerMonth;
        // outdoor tea bush cannot shake in winter
        if (!bush.IsSheltered() && Game1.Date.SeasonIndex + nextSeason == (int)Season.Winter) {
            daysLeft += WorldDate.DaysPerMonth;
        }

        return daysLeft;
    }
}