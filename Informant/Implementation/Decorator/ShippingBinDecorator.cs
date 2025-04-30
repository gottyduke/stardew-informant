using Microsoft.Xna.Framework.Graphics;
using Slothsoft.Informant.Api;
using Slothsoft.Informant.Implementation.Common;
using StardewModdingAPI.Events;
using StardewValley.GameData.Objects;

namespace Slothsoft.Informant.Implementation.Decorator;

internal class ShippingBinDecorator : IDecorator<Item>
{
    public const int PolycultureAmount = 15;
    public const int SingleShippingAmount = 1;

    private static Texture2D? _shippingBin;
    private static HashSet<string>? polycultureItemIds = null;
    private static HashSet<string> PolycultureItemIds => polycultureItemIds ??= Game1.cropData.Values.Where(cropData => cropData.CountForPolyculture).Select(cropData => cropData.HarvestItemId).ToHashSet();
    private readonly IModHelper _modHelper;

    public ShippingBinDecorator(IModHelper modHelper, string? uniqueId = null)
    {
        _modHelper = modHelper;
        _shippingBin ??= modHelper.ModContent.Load<Texture2D>("assets/shipping_bin.png");
        _modHelper.Events.Content.AssetsInvalidated += OnAssetInvalidated;
    }

    private void OnAssetInvalidated(object? sender, AssetsInvalidatedEventArgs e)
    {
        if (e.Names.Any(name => name.IsEquivalentTo("Data/Crops")))
            polycultureItemIds = null;
    }

    public string Id => "shipping";
    public string DisplayName => _modHelper.Translation.Get("ShippingBinDecorator");
    public string Description => _modHelper.Translation.Get("ShippingBinDecorator.Description");

    public bool HasDecoration(Item input)
    {
        if (_shippingBin == null || input is not SObject obj || obj.bigCraftable.Value)
        {
            return false;
        }

        return CalculateStillNeeded(input) != null;
    }

    private static int? CalculateStillNeeded(Item input)
    {
        if (input is not SObject obj)
        {
            return null;
        }

        var itemId = input.ItemId;
        var trackingType = InformantMod.Instance?.Config.ShippingBinTracking ?? ShippingBinTrackingType.Collection;
        // 15 of each crop
        var amountNeeded = PolycultureItemIds.Contains(itemId) ? PolycultureAmount : SingleShippingAmount;
        var alreadyShipped = Game1.player.basicShipped.ContainsKey(itemId) ? Game1.player.basicShipped[itemId] : 0;
        var amountStillNeeded = amountNeeded - alreadyShipped;
        if (amountStillNeeded <= 0)
        {
            // we don't need to ship anything? why even show the decorator?
            return null;
        }

        if (trackingType == ShippingBinTrackingType.All || (trackingType == ShippingBinTrackingType.Collection && SObject.isPotentialBasicShipped(input.ItemId, input.Category, obj.Type)))
        {
            return amountStillNeeded;
        }

        return null;
    }

    public Decoration Decorate(Item input)
    {
        var counter = CalculateStillNeeded(input);
        return new Decoration(_shippingBin!)
        {
            Counter = counter == 1 ? null : counter,
        };
    }
}
