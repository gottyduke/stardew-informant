using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using Slothsoft.Informant.Api;
using Slothsoft.Informant.Implementation.Common;
using StardewValley.Menus;

namespace Slothsoft.Informant.Implementation.Decorator;

internal class ShippingBinDecorator : IDecorator<Item>
{
    public const int PolycultureAmount = 15;
    public const int SingleShippingAmount = 1;

    private static Texture2D? _shippingBin;
    private static HashSet<string>? _collections;

    private readonly IModHelper _modHelper;
    private readonly Harmony _harmony;

    public ShippingBinDecorator(IModHelper modHelper, string? uniqueId = null)
    {
        _modHelper = modHelper;
        _shippingBin ??= modHelper.ModContent.Load<Texture2D>("assets/shipping_bin.png");
        _harmony = new Harmony(uniqueId ?? InformantMod.Instance!.ModManifest.UniqueID);
        _harmony.Patch(
            original: AccessTools.Method(
                typeof(GameMenu),
                nameof(GameMenu.draw),
                [
                    typeof(SpriteBatch),
                ]
            ),
            postfix: new HarmonyMethod(typeof(ShippingBinDecorator), nameof(GetFlattenedCollections))
        );
    }

    public string Id => "shipping";
    public string DisplayName => _modHelper.Translation.Get("ShippingBinDecorator");
    public string Description => _modHelper.Translation.Get("ShippingBinDecorator.Description");

    public bool HasDecoration(Item input)
    {
        if (_shippingBin == null || input is not SObject obj || obj.bigCraftable.Value) {
            return false;
        }

        return CalculateStillNeeded(input) != null;
    }

    private static int? CalculateStillNeeded(Item input)
    {
        var itemId = input.ItemId;
        var trackingType = InformantMod.Instance?.Config.ShippingBinTracking ?? ShippingBinTrackingType.Collection;
        // 15 of each crop
        var amountNeeded = CropIds.Polyculture.Contains(itemId) ? PolycultureAmount : SingleShippingAmount;
        var alreadyShipped = Game1.player.basicShipped.ContainsKey(itemId) ? Game1.player.basicShipped[itemId] : 0;
        var amountStillNeeded = amountNeeded - alreadyShipped;
        if (amountStillNeeded <= 0) {
            // we don't need to ship anything? why even show the decorator?
            return null;
        }

        // track all shippables or collection page only
        if ((trackingType == ShippingBinTrackingType.All && input is SObject obj && obj.countsForShippedCollection()) ||
            (trackingType == ShippingBinTrackingType.Collection && _collections != null && _collections.Contains(itemId))) {
            return amountStillNeeded;
        }

        return null;
    }

    private static void GetFlattenedCollections(GameMenu __instance)
    {
        if (_collections != null || __instance == null) {
            // already acquired
            return;
        }

        if (__instance.pages[GameMenu.collectionsTab] is CollectionsPage collectionsPage) {
            _collections = collectionsPage.collections.Values
                .SelectMany(tab => tab)
                .SelectMany(page => page)
                .Select(item => item.name.Split(' ')[0])
                .ToHashSet();
        }
    }

    public Decoration Decorate(Item input)
    {
        var counter = CalculateStillNeeded(input);
        return new Decoration(_shippingBin!) {
            Counter = counter == 1 ? null : counter,
        };
    }
}
