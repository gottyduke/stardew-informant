using Microsoft.Xna.Framework.Graphics;
using Slothsoft.Informant.Api;
using Slothsoft.Informant.Helper;
using StardewValley.Locations;
using StardewValley.Menus;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;


namespace Slothsoft.Informant.Implementation.Decorator;

internal class BundleDecorator : IDecorator<Item>
{
    internal record ParsedSimpleBundle
    {
        public int ParentSheetIndex;
        public int Quantity;
        public int Quality;
        public int Color;
    }

    public const int DefaultBundleColor = -1;

    private static Texture2D? _bundle;
    private static Dictionary<int, Texture2D> _bundles = [];
    private static IEnumerable<ParsedSimpleBundle>? _lastCachedBundles;

    private readonly IModHelper _modHelper;

    public BundleDecorator(IModHelper modHelper)
    {
        _modHelper = modHelper;
        _bundle ??= Game1.temporaryContent.Load<Texture2D>("LooseSprites\\JunimoNote");
        _bundles[DefaultBundleColor] = modHelper.ModContent.Load<Texture2D>("assets/bundle.png");
    }

    public string Id => "bundles";
    public string DisplayName => _modHelper.Translation.Get("BundleTooltipDecorator");
    public string Description => _modHelper.Translation.Get("BundleTooltipDecorator.Description");

    public bool HasDecoration(Item input)
    {
        if (Game1.MasterPlayer.mailReceived.Contains("JojaMember")) {
            return false;
        }

        if (_bundles.Any() && input is SObject obj && !obj.bigCraftable.Value) {
            int[]? allowedAreas;

            if (!Game1.MasterPlayer.mailReceived.Contains("canReadJunimoText")) {
                // if player can't read Junimo text, they can't have bundles yet
                allowedAreas = null;
            } else {
                // let the community center calculate which bundles are allowed
                var communityCenter = Game1.getLocationFromName("CommunityCenter") as CommunityCenter;
                allowedAreas = communityCenter?.areasComplete
                    .Select((complete, index) => new { complete, index })
                    .Where(area => communityCenter.shouldNoteAppearInArea(area.index) && !area.complete)
                    .Select(area => area.index)
                    .ToArray();
            }

            _lastCachedBundles = GetNeededItems(allowedAreas, InformantMod.Instance?.Config.DecorateLockedBundles ?? false)
                .Where(item => input.ParentSheetIndex == item.ParentSheetIndex && input.quality.Value <= item.Quality);
            return _lastCachedBundles.Any();
        }
        return false;
    }

    internal static IEnumerable<ParsedSimpleBundle> GetNeededItems(int[]? allowedAreas, bool decorateLockedBundles)
    {
        // BUNDLE DATA
        // ============
        // See https://stardewvalleywiki.com/Modding:Bundles
        // The "main" data of the bundle has three values per item:
        // ParentSheetIndex Stack Quality (-> BundleGenerator.ParseItemList)
        //
        // Examples:
        //
        // bundleTitle = Pantry/0
        // bundleData = Spring Crops/O 465 20/24 1 0 188 1 0 190 1 0 192 1 0/0/4/0
        //
        // bundleTitle = Boiler Room/22
        // bundleData = Adventurer's/R 518 1/766 99 0 767 10 0 768 1 0 881 10 0/1/2/22

        if ((allowedAreas == null || allowedAreas.Length == 0) && !decorateLockedBundles) {
            // no areas are allowed, and we don't decorate locked bundles; so no bundle is needed yet
            yield break;
        }

        var bundleData = Game1.netWorldState.Value.BundleData;
        var bundlesCompleted = Game1.netWorldState.Value.Bundles.Pairs
            .ToDictionary(p => p.Key, p => p.Value.ToArray());

        foreach (var bundleTitle in bundleData.Keys) {
            var bundleTitleSplit = bundleTitle.Split('/');
            var bundleTitleId = bundleTitleSplit[0];
            if ((allowedAreas != null && !allowedAreas.Contains(CommunityCenter.getAreaNumberFromName(bundleTitleId))) && !decorateLockedBundles) {
                // bundle was not yet unlocked or already completed
                continue;
            }
            
            _ = int.TryParse(bundleTitleSplit[1], out var bundleIndex);
            var bundleDataSplit = bundleData[bundleTitle].Split('/');
            var indexStackQuality = bundleDataSplit[2].Split(' ');
            for (var index = 0; index < indexStackQuality.Length; index += 3) {
                if (!bundlesCompleted[bundleIndex][index / 3]) {
                    if (int.TryParse(indexStackQuality[index], out var parentSheetIndex)) {
                        _ = int.TryParse(indexStackQuality[index + 1], out var quantity);
                        _ = int.TryParse(indexStackQuality[index + 2], out var quality);
                        _ = int.TryParse(bundleDataSplit[3], out var color);
                        yield return new ParsedSimpleBundle { 
                            ParentSheetIndex = parentSheetIndex,
                            Quantity = quantity,
                            Quality = quality,
                            Color = color,
                        };
                    }
                }
            }
        }
    }

    internal static Texture2D GetOrCacheBundleTexture(int? color)
    {
        var colorIndex = color ?? DefaultBundleColor;
        if (!_bundles.ContainsKey(colorIndex)) {
            var rect = new Rectangle(colorIndex * 256 % 512, 244 + colorIndex * 256 / 512 * 16, 16, 16);
            _bundles[colorIndex] = _bundle!.Blit(rect);
        }

        return _bundles[colorIndex];
    }

    public Decoration Decorate(Item input)
    {
        var decorations = _lastCachedBundles!
            .Skip(1)
            .Select(bundle => new Decoration(GetOrCacheBundleTexture(bundle.Color)) { Counter = bundle.Quantity })
            .ToArray();
        return new Decoration(GetOrCacheBundleTexture(_lastCachedBundles!.First().Color)) {
            Counter = _lastCachedBundles!.First().Quantity,
            ExtraDecorations = decorations
        };
    }
}