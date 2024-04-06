using Microsoft.Xna.Framework.Graphics;
using Slothsoft.Informant.Api;
using Slothsoft.Informant.Implementation.Common;

namespace Slothsoft.Informant.Implementation.Decorator;

internal class ShippingBinDecorator : IDecorator<Item>
{
    private static Texture2D? _shippingBin;

    private readonly IModHelper _modHelper;

    public ShippingBinDecorator(IModHelper modHelper)
    {
        _modHelper = modHelper;
        _shippingBin ??= modHelper.ModContent.Load<Texture2D>("assets/shipping_bin.png");
    }

    public string Id => "shipping";
    public string DisplayName => _modHelper.Translation.Get("ShippingBinDecorator");
    public string Description => _modHelper.Translation.Get("ShippingBinDecorator.Description");

    public bool HasDecoration(Item input)
    {
        if (_shippingBin != null && input is SObject obj && !obj.bigCraftable.Value) {
            if (!obj.countsForShippedCollection()) {
                // we do not need to ship this item
                return false;
            }
            var alreadyShipped = Game1.player.basicShipped.ContainsKey(input.ItemId) ? Game1.player.basicShipped[input.ItemId] : 0;

            if (!CropIds.Polyculture.Contains(input.ItemId)) {
                // we only need to ship this item once
                return alreadyShipped == 0;
            }
            const int needToBeShipped = 15;
            // we need to ship this item 15 times
            return alreadyShipped < needToBeShipped;
        }
        return false;
    }

    public Decoration Decorate(Item input)
    {
        return new Decoration(_shippingBin!) {
            Counter = CalculateStillNeeded(input),
        };
    }

    public int? CalculateStillNeeded(Item input)
    {
        if (!CropIds.Polyculture.Contains(input.ItemId)) {
            // we don't need to show any number because we don't need to ship 15
            return null;
        }
        var parentSheetIndex = input.ParentSheetIndex.ToString();
        var alreadyShipped = Game1.MasterPlayer.basicShipped.ContainsKey(parentSheetIndex) ? Game1.MasterPlayer.basicShipped[parentSheetIndex] : 0;
        const int needToBeShipped = 15;
        if (alreadyShipped >= needToBeShipped) {
            // we don't need to ship anything? why even show the decorator?
            return null;
        }
        return needToBeShipped - alreadyShipped;
    }
}