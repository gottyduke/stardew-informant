using Microsoft.Xna.Framework.Graphics;
using Slothsoft.Informant.Api;
using Slothsoft.Informant.Helper;
using Slothsoft.Informant.Implementation.Common;
using StardewValley.Locations;

namespace Slothsoft.Informant.Implementation.Decorator;

internal class SeedDecorator : IDecorator<Item>
{
    public const int SeedCategory = -74;

    private static readonly Dictionary<string, Texture2D> CropTextures = [];
    private static readonly Dictionary<string, ParsedSimpleSeed> CachedCropData = [];

    private readonly IModHelper _modHelper;

    public SeedDecorator(IModHelper modHelper)
    {
        _modHelper = modHelper;
    }

    public string Id => "seed";
    public string DisplayName => _modHelper.Translation.Get("SeedTooltipDecorator");
    public string Description => _modHelper.Translation.Get("SeedTooltipDecorator.Description");

    public bool HasDecoration(Item input)
    {
        if (input.Category != SeedCategory) {
            // must be a seed type
            return false;
        }

        if (CachedCropData.ContainsKey(input.ItemId)) {
            // already cached
            return true;
        }

        var crops = GetCropFromSeed(input.ItemId,
                Game1.player.currentLocation is IslandLocation ? Game1.player.currentLocation : Game1.getFarm())
            .ToArray();
        var valid = crops.Length > 0;
        if (valid) {
            CachedCropData[input.ItemId] = new(input.ItemId, crops);
        }

        return valid;
    }

    public Decoration Decorate(Item input)
    {
        var decorations = CachedCropData[input.ItemId].HarvestId
            .Select(crop => {
                _ = ItemRegistry.GetDataOrErrorItem(crop);
                var monoItem = ItemRegistry.Create(crop);
                return new Decoration(GetOrCacheCropTexture(crop)) {
                    Counter = Utility.getSellToStorePriceOfItem(monoItem),
                };
            })
            .ToArray();
        return decorations[0] with { ExtraDecorations = decorations[1..] };
    }

    internal static IEnumerable<string> GetCropFromSeed(string seedId, GameLocation seedLocation)
    {
        var seedType = SeedType.Crop;

        string[] variantSeed = [seedId];
        var season = seedLocation.GetSeason();

        switch (seedId) {
            case CropIds.MixedFlowers:
                // random mixed flowers
                variantSeed = season switch {
                    Season.Spring => ["427", "429"],
                    Season.Summer => ["455", "453", "431"],
                    Season.Fall => ["431", "425"],
                    _ => [seedId],
                };
                break;
            case CropIds.MixedSeeds: {
                // random mixed seeds
                variantSeed = season switch {
                    Season.Spring => ["472", "474", "475", "476"],
                    Season.Summer => ["482", "483", "484", "487"],
                    Season.Fall => ["487", "488", "489", "490", "491"],
                    _ => [seedId],
                };

                if (seedLocation is IslandLocation) {
                    variantSeed = ["478", "481", "479", "833"];
                }

                break;
            }
            default: {
                if (Game1.fruitTreeData.ContainsKey(seedId)) {
                    // could this be a fruit tree?
                    seedType = SeedType.FruitTree;
                } else {
                    seedType = seedId switch {
                        CropIds.MossySeed => SeedType.Moss,
                        CropIds.GreenTeaBush => SeedType.TeaBush,
                        _ => seedType,
                    };
                }

                break;
            }
        }

        foreach (var variant in variantSeed) {
            switch (seedType) {
                case SeedType.Crop:
                    if (Crop.TryGetData(variant, out var crop) && crop.Seasons.Count != 0) {
                        yield return crop.HarvestItemId;
                    }

                    break;
                case SeedType.FruitTree:
                    foreach (var fruit in Game1.fruitTreeData[variant].Fruit) {
                        yield return fruit.ItemId;
                    }

                    break;
                case SeedType.TeaBush:
                    // CUSTOM BUSH WILL BREAK THIS
                    yield return CropIds.GreenTeaLeaves;
                    break;
                case SeedType.Moss:
                    yield return CropIds.Moss;
                    break;
            }
        }
    }

    internal static Texture2D GetOrCacheCropTexture(string cropId)
    {
        var crop = ItemRegistry.GetDataOrErrorItem(cropId);
        if (!CropTextures.ContainsKey(cropId)) {
            CropTextures[cropId] = crop.GetTexture().Blit(crop.GetSourceRect());
        }

        return CropTextures[cropId];
    }

    private enum SeedType
    {
        Crop,
        FruitTree,
        TeaBush,
        Moss,
    }

    internal record ParsedSimpleSeed(string SeedId, string[] HarvestId);
}