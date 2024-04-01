using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Slothsoft.Informant.Api;
using StardewValley.Menus;
using System.Collections.Immutable;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Slothsoft.Informant.Implementation;

internal class ItemDecoratorManager : IDecoratorManager<Item>
{

    private static readonly List<IDecorator<Item>> DecoratorsList = [];
    private static Rectangle? _lastToolTipCoordinates;

    private readonly Harmony _harmony;

    public ItemDecoratorManager(IModHelper modHelper)
    {
        _harmony = new Harmony(InformantMod.Instance!.ModManifest.UniqueID);
        _harmony.Patch(
            original: AccessTools.Method(
                typeof(IClickableMenu),
                nameof(IClickableMenu.drawToolTip)
            ),
            postfix: new HarmonyMethod(typeof(ItemDecoratorManager), nameof(DrawToolTip))
        );
        _harmony.Patch(
            original: AccessTools.Method(
                typeof(IClickableMenu),
                nameof(IClickableMenu.drawTextureBox),
                [
                    typeof(SpriteBatch),
                    typeof(Texture2D),
                    typeof(Rectangle),
                    typeof(int),
                    typeof(int),
                    typeof(int),
                    typeof(int),
                    typeof(Color),
                    typeof(float),
                    typeof(bool),
                    typeof(float)
                ]
            ),
            postfix: new HarmonyMethod(typeof(ItemDecoratorManager), nameof(RememberToolTipCoordinates))
        );
    }

    private static void DrawToolTip(SpriteBatch b, Item? hoveredItem)
    {
        if (_lastToolTipCoordinates == null || hoveredItem == null) {
            return;
        }

        var config = InformantMod.Instance?.Config ?? new InformantConfig();
        var decorations = DecoratorsList
            .Where(g => config.DisplayIds.GetValueOrDefault(g.Id, true))
            .Where(d => d.HasDecoration(hoveredItem))
            .SelectMany(d => {
                var decoration = d.Decorate(hoveredItem);
                Decoration[] decorations = [
                    decoration,
                    .. decoration.ExtraDecorations ?? [],
                ];
                return decorations;
            })
            .ToArray();

        if (decorations.Length == 0) {
            return;
        }

        var tipCoordinates = _lastToolTipCoordinates.Value;
        const int spacing = Game1.pixelZoom;
        const int borderSize = 3 * spacing;
        const int indent = 4 * spacing;
        const int decoratorsHeight = Game1.tileSize;
        const int decorationPerTile = decoratorsHeight - 2 * indent;

        var decoratorsBox = new Rectangle(tipCoordinates.X, tipCoordinates.Y - decoratorsHeight + borderSize, tipCoordinates.Width, decoratorsHeight);
        // extend the box vertically if necessary
        var rows = 1;
        var drawableWidth = decoratorsBox.Width - 2 * (indent + borderSize);
        var decoratorWidthPerRow = 0;
        foreach (var decorator in decorations) {
            var widthRequired = decorator.Texture.Width + spacing;
            if (widthRequired >= drawableWidth) {
                // what is this texture...
                continue;
            }
            decoratorWidthPerRow += widthRequired;
            if (decoratorWidthPerRow > drawableWidth) {
                rows++;
                decoratorWidthPerRow = widthRequired;
            }
        }
        if (rows > 1 && decoratorWidthPerRow > 0) {
            // remainder fits on the last row
            rows++;
        }
        // fit the extra rows
        var extraRows = rows - 1;
        decoratorsBox.Y -= extraRows * (decorationPerTile + spacing);
        decoratorsBox.Height += extraRows * (decorationPerTile + spacing);

        IClickableMenu.drawTextureBox(b, Game1.menuTexture, TooltipGeneratorManager.TooltipSourceRect, decoratorsBox.X,
            decoratorsBox.Y, decoratorsBox.Width, decoratorsBox.Height, Color.White, drawShadow: false);

        var destinationRectangle = new Rectangle(
            decoratorsBox.X + indent,
            decoratorsBox.Y + indent,
            decorationPerTile,
            decorationPerTile
        );
        var beginning = destinationRectangle;

        decoratorWidthPerRow = 0;
        foreach (var decoration in decorations) {
            // fold to next row if need be
            if (decoratorWidthPerRow + destinationRectangle.Width > drawableWidth) {
                destinationRectangle.X = beginning.X;
                destinationRectangle.Y = destinationRectangle.Y + destinationRectangle.Height + spacing;
                decoratorWidthPerRow = 0;
            }

            // give numbers more than 2 digits a little more spacing
            var counter = decoration.Counter;
            if (counter != null) {
                destinationRectangle.X += NumberSprite.getWidth(counter.Value) - 2 * NumberSprite.digitWidth;
            }

            b.Draw(decoration.Texture, destinationRectangle, null, Color.White);

            if (counter != null) {
                const float scale = 0.5f;
                // these x and y coordinates are the top left of the right-most number of the counter
                var x = destinationRectangle.X + destinationRectangle.Width - NumberSprite.getWidth(counter.Value % 10) + 2;
                var y = destinationRectangle.Y + destinationRectangle.Height - NumberSprite.getHeight() + 2;
                NumberSprite.draw(counter.Value, b, new Vector2(x, y), decoration.CounterColor ?? Color.White, scale, 1, 1, 0);
            }

            destinationRectangle.X += destinationRectangle.Width + spacing;
            decoratorWidthPerRow += destinationRectangle.Width + spacing;
        }
    }

    private static void RememberToolTipCoordinates(int x, int y, int width, int height)
    {
        _lastToolTipCoordinates = new Rectangle(x, y, width, height);
    }

    public IEnumerable<IDisplayable> Decorators => DecoratorsList.ToImmutableArray();

    public void Add(IDecorator<Item> decorator)
    {
        DecoratorsList.Add(decorator);
    }

    public void Remove(string decoratorId)
    {
        DecoratorsList.RemoveAll(g => g.Id == decoratorId);
    }
}