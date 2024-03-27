using Microsoft.Xna.Framework.Graphics;
using Slothsoft.Informant.Api;
using StardewValley.Locations;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Slothsoft.Informant.Implementation.Decorator;

internal class BundleDecorator : IDecorator<Item>
{
    private static Texture2D? _bundle;
    private static Dictionary<int, Texture2D> _bundles = [];
    private static int? _lastCachedItemQuantity;
    private static int? _lastCachedBundleColor;

    private readonly IModHelper _modHelper;

    public BundleDecorator(IModHelper modHelper)
    {
        _modHelper = modHelper;
        _bundle ??= Game1.temporaryContent.Load<Texture2D>("LooseSprites\\JunimoNote");
        _bundles[-1] = modHelper.ModContent.Load<Texture2D>("assets/bundle.png");
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

            var neededItems = GetNeededItems(allowedAreas, InformantMod.Instance?.Config.DecorateLockedBundles ?? false)
                .Where(item => item[0] == input.ParentSheetIndex);
            if (neededItems.Any()) {
                _lastCachedItemQuantity = neededItems.First()[1];
                _lastCachedBundleColor = neededItems.First()[2];
            } else {
                _lastCachedItemQuantity = null;
                _lastCachedBundleColor = null;
            }

            return neededItems.Any();
        }
        return false;
    }

    internal static IEnumerable<int[]> GetNeededItems(int[]? allowedAreas, bool decorateLockedBundles)
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
                        GetOrCacheBundleTexture(int.TryParse(bundleDataSplit[3], out var color) ? color : null);
                        yield return [parentSheetIndex, quantity, color];
                    }
                }
            }
        }
    }

    internal static Texture2D GetOrCacheBundleTexture(int? color)
    {
        var colorIndex = color ?? -1;
        if (_bundles.ContainsKey(colorIndex)) {
            return _bundles[colorIndex];
        }

        var rect = new Rectangle(colorIndex * 256 % 512, 244 + colorIndex * 256 / 512 * 16, 16, 16);
        var note = new Texture2D(Game1.graphics.GraphicsDevice, 16, 16);
        Color[] data = new Color[256];

        _bundle!.GetData(0, rect, data, 0, data.Length);
        note.SetData(data);
        _bundles[colorIndex] = note;

        return note;
    }

    public Decoration Decorate(Item input)
    {
        return new Decoration(GetOrCacheBundleTexture(_lastCachedBundleColor)) { Counter = _lastCachedItemQuantity };
    }
}