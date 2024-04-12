﻿using Slothsoft.Informant.Api;
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

    public bool HasTooltip(TerrainFeature input)
    {
        return input is Tree;
    }

    public Tooltip Generate(TerrainFeature input)
    {
        return new Tooltip(CreateText((Tree)input));
    }

    private string CreateText(Tree tree)
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
                treeString = _modHelper.Translation.Get("TreeTooltipGenerator.Type" + tree.treeType.Value);
                if (tree.hasMoss.Value) {
                    treeString = _modHelper.Translation.Get("TreeTooltipGenerator.MossCovered", new { X = treeString });
                }
                break;
            case Tree.greenRainTreeBushy:
            case Tree.greenRainTreeLeafy:
            case Tree.greenRainTreeFern:
                treeString = _modHelper.Translation.Get("TreeTooltipGenerator.Type10");
                break;
            default:
                break;
        }

        if (InformantMod.Instance?.Config.ShowTreeGrowthStage ?? false) {
            treeString += $" {GetTreeGrowthStage(_modHelper, tree)}";
        }

        return treeString;
    }

    internal static string GetTreeGrowthStage(IModHelper modHelper, TerrainFeature treeFeature)
    {
        var growthStage = -1;
        if (treeFeature is Tree tree) {
            growthStage = tree.growthStage.Value == Tree.treeStage ? FruitTree.treeStage : tree.growthStage.Value;
        }
        if (treeFeature is FruitTree fruitTree) {
            growthStage = fruitTree.growthStage.Value;
        }

        return growthStage != -1
            ? modHelper.Translation.Get($"TreeTooltipGenerator.ShowGrowthStage.{growthStage}")
            : "???";
    }
}
