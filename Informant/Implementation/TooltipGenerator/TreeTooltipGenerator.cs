using Microsoft.Xna.Framework;
using Slothsoft.Informant.Api;
using Slothsoft.Informant.Implementation.Common;
using StardewValley.TerrainFeatures;

namespace Slothsoft.Informant.Implementation.TooltipGenerator;

internal class TreeTooltipGenerator : ITooltipGenerator<TerrainFeature>
{

    private readonly IModHelper _modHelper;

    public TreeTooltipGenerator(IModHelper modHelper)
    {
        _modHelper = modHelper;
    }

    public string Id => "tree";
    public string DisplayName => _modHelper.Translation.Get("TreeTooltipGenerator");
    public string Description => _modHelper.Translation.Get("TreeTooltipGenerator.Description");
    public string FertilizerId = "805";
    
    public bool HasTooltip(TerrainFeature input)
    {
        return input is Tree;
    }

    public Tooltip Generate(TerrainFeature input)
    {
        return CreateTooltip(_modHelper, (Tree)input);
    }

    internal Tooltip CreateTooltip(IModHelper modHelper, Tree tree) {
        var text = CreateText(modHelper, tree);
        var fertilizerIcon = tree.growthStage.Value < Tree.treeStage && tree.fertilized.Value && (InformantMod.Instance?.Config.DecorateTreeFertilizer ?? false) ?
                Icon.ForUnqualifiedItemId(
                    FertilizerId,
                    IPosition.CenterRight,
                    new Vector2(Game1.tileSize / 2f, Game1.tileSize / 2f)
                ) : null;
        return new Tooltip(text) {
            Icon = [
                fertilizerIcon,
            ]
        };
    }

    private string CreateText(IModHelper modHelper, Tree tree)
    {
        var treeString = "???";
        switch (tree.treeType.Value) {
            case Tree.bushyTree:
            case Tree.leafyTree:
            case Tree.pineTree:
            case Tree.palmTree:
            case Tree.mushroomTree:
            case Tree.mahoganyTree:
            case Tree.palmTree2:
            case Tree.mysticTree:
            case SveTreeIds.BirchTree:
            case SveTreeIds.FirTree:
            case VmvTreeIds.BirchTree:
            case VmvTreeIds.AmberTree:
            case VmvTreeIds.HazelnutTree:
            case VmvTreeIds.BlackChanterelleTree:
            case VmvTreeIds.SkyshardPineTree:
            case WagTreeIds.MysticTrumpetTree:
            case WagTreeIds.StrangeInkCapTree:
            case WagTreeIds.WitchwoodTree:
            case PcTreeIds.ReuuTree:
            case PcTreeIds.MeekTree:
            case PcTreeIds.HeaviousTree:
                treeString = modHelper.Translation.Get("TreeTooltipGenerator.Type" + tree.treeType.Value);
                if (tree.hasMoss.Value) {
                    treeString = modHelper.Translation.Get("TreeTooltipGenerator.MossCovered", new { X = treeString });
                }
                break;
            case Tree.greenRainTreeBushy:
            case Tree.greenRainTreeLeafy:
            case Tree.greenRainTreeFern:
                treeString = modHelper.Translation.Get("TreeTooltipGenerator.Type10");
                break;
            default:
                break;
        }

        if (InformantMod.Instance?.Config.ShowTreeGrowthStage ?? false) {
            treeString += GetTreeGrowthStage(modHelper, tree);
        }

        return treeString;
    }

    internal static string GetTreeGrowthStage(IModHelper modHelper, TerrainFeature treeFeature)
    {
        var growthStage = -1;
        var treeStage = Tree.treeStage;
        if (treeFeature is Tree tree) {
            growthStage = tree.growthStage.Value;
        }
        if (treeFeature is FruitTree fruitTree) {
            growthStage = fruitTree.growthStage.Value;
            treeStage = FruitTree.treeStage;
        }
        var stageString = growthStage < treeStage
            ? modHelper.Translation.Get("TreeTooltipGenerator.ShowGrowthStage.GrowthStage", new {
                N = growthStage,
                T = treeStage,
            })
            : modHelper.Translation.Get("TreeTooltipGenerator.ShowGrowthStage.MatureStage");

        return $"\n({stageString})";
    }
}
