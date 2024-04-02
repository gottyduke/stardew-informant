using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Slothsoft.Informant.Api;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley.Characters;
using StardewValley.Menus;
using StardewValley.TerrainFeatures;
using System.Collections.Immutable;

namespace Slothsoft.Informant.Implementation;

/// <summary>
/// Generating tooltips for all kinds of objects is actually pretty similar, that's why this
/// is only one instance for all <see cref="ITooltipGeneratorManager{TInput}"/> implementations.
/// </summary>

internal class TooltipGeneratorManager : ITooltipGeneratorManager<TerrainFeature>, ITooltipGeneratorManager<SObject>, ITooltipGeneratorManager<Character>
{

    internal static Rectangle TooltipSourceRect = new(0, 256, 60, 60);

    private readonly IModHelper _modHelper;
    private BaseTooltipGeneratorManager<TerrainFeature>? _terrainFeatureManager;
    private BaseTooltipGeneratorManager<SObject>? _objectInformant;
    private BaseTooltipGeneratorManager<Character>? _characterInformant;
    private BaseTooltipGeneratorManager<Pet>? _petInformant;

    private readonly PerScreen<IEnumerable<Tooltip>?> _tooltips = new();

    public TooltipGeneratorManager(IModHelper modHelper)
    {
        _modHelper = modHelper;

        modHelper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
        modHelper.Events.Display.Rendered += OnRendered;
    }

    IEnumerable<IDisplayable> ITooltipGeneratorManager<TerrainFeature>.Generators =>
        _terrainFeatureManager?.Generators.ToImmutableArray() ?? Enumerable.Empty<IDisplayable>();

    IEnumerable<IDisplayable> ITooltipGeneratorManager<SObject>.Generators =>
        _objectInformant?.Generators.ToImmutableArray() ?? Enumerable.Empty<IDisplayable>();

    IEnumerable<IDisplayable> ITooltipGeneratorManager<Character>.Generators =>
        _characterInformant?.Generators.ToImmutableArray() ?? Enumerable.Empty<IDisplayable>();

    private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e2)
    {
        if (!Context.IsPlayerFree) {
            _tooltips.Value = [];
            return;
        }

        if (!WasTriggered()) {
            _tooltips.Value = [];
            return;
        }

        _tooltips.Value = [
            .. GenerateTerrainFeatureTooltips(),
            .. GenerateObjectTooltips(),
            .. GenerateCharacterTooltips(),
        ];
    }

    private bool WasTriggered()
    {
        var config = InformantMod.Instance?.Config;
        if (config == null) {
            return false; // something went wrong here!
        }

        var shouldRender = config.TooltipTrigger switch {
            TooltipTrigger.Hover => true, // hover is ALWAYS triggered
            TooltipTrigger.ButtonHeld => _modHelper.Input.GetState(config.TooltipTriggerButton) == SButtonState.Held,
            _ => false // we don't know this trigger
        };

        return shouldRender && (!Game1.options.gamepadControls || Game1.timerUntilMouseFade > 0);
    }

    private IEnumerable<Tooltip> GenerateTerrainFeatureTooltips()
    {
        return GenerateTooltips(_terrainFeatureManager, (mouseX, mouseY) =>
            Game1.player.currentLocation.terrainFeatures.Values
                .Where(t => t.Tile == GetTilePosition(mouseX, mouseY))
                .ToArray()
        );
    }

    private IEnumerable<Tooltip> GenerateObjectTooltips()
    {
        return GenerateTooltips(_objectInformant, (mouseX, mouseY) =>
            Game1.player.currentLocation.netObjects.Values
                .Where(o => o.TileLocation == GetTilePosition(mouseX, mouseY))
                .ToArray()
        );
    }

    private IEnumerable<Tooltip> GenerateCharacterTooltips()
    {
        return GenerateTooltips(_characterInformant, (mouseX, mouseY) => [
            .. Game1.player.currentLocation.characters
                .Where(c => c.GetBoundingBox().Contains(mouseX + Game1.viewport.X, mouseY + Game1.viewport.Y)),
            .. Game1.player.currentLocation.animals.Values
                .Where(a => a.GetCursorPetBoundingBox().Contains(mouseX + Game1.viewport.X, mouseY + Game1.viewport.Y)),
        ]);
    }

    private void OnRendered(object? sender, RenderedEventArgs e)
    {
        if (Context.IsPlayerFree && _tooltips.Value != null) {
            var tooltipsArray = _tooltips.Value.ToArray();
            if (tooltipsArray.Length == 0) {
                return;
            }
            const int borderSize = 3 * Game1.pixelZoom;
            var font = Game1.smallFont;
            var approximateBounds = CalculateApproximateBounds(tooltipsArray, font);
            var extendedBounds = ApplyTooltipIconPositions(approximateBounds, tooltipsArray);

            // move both bounds to the right and bottom if the tooltip was extended to the left and / or above
            var diffX = approximateBounds.X - extendedBounds.X;
            var diffY = approximateBounds.Y - extendedBounds.Y;
            approximateBounds.X += diffX;
            approximateBounds.Y += diffY;
            extendedBounds.X += diffX;
            approximateBounds.Y += diffY;

            // now we have all the data to create perfect little tooltip
            var startY = approximateBounds.Y;

            foreach (var tooltip in tooltipsArray) {
                var height = Math.Max(60, (int)font.MeasureString(tooltip.Text).Y + Game1.tileSize / 2);
                DrawSimpleTooltip(Game1.spriteBatch, tooltip, font, extendedBounds with {
                    Y = startY,
                    Height = height,
                }, new Vector2(approximateBounds.X, startY));
                startY += height - borderSize;
            }
        }
    }

    private static Vector2 GetTilePosition(int x, int y)
    {
        var posX = (x + Game1.viewport.X) / Game1.tileSize;
        var posY = (y + Game1.viewport.Y) / Game1.tileSize;
        return new Vector2(posX, posY);
    }

    private static IEnumerable<Tooltip> GenerateTooltips<TTile>(BaseTooltipGeneratorManager<TTile>? manager, Func<int, int, TTile[]> getTilesForBounds)
    {
        if (manager == null) {
            // if there is no generator in that, we don't need to do anything further
            return [];
        }

        var mouseX = Game1.getOldMouseX();
        var mouseY = Game1.getOldMouseY();

        var toolbar = Game1.onScreenMenus.FirstOrDefault(m => m is Toolbar);
        if (toolbar != null && toolbar.isWithinBounds(mouseX, mouseY)) {
            // mouse is over the toolbar, so we won't generate tooltips for the map
            return [];
        }

        return manager.Generate(getTilesForBounds(mouseX, mouseY));
    }

    private static Rectangle CalculateApproximateBounds(Tooltip[] tooltips, SpriteFont font)
    {
        // this join with two linebreaks between the tooltips is a pretty good approximation (for English and German at least)
        var textSize = font.MeasureString(string.Join("\n\n", tooltips.Select(t => t.Text)));
        var height = Math.Max(60, textSize.Y + Game1.tileSize / 2);
        var x = Game1.getOldMouseX() + Game1.tileSize / 2;
        var y = Game1.getOldMouseY() + Game1.tileSize / 2;

        if (x + textSize.X > Game1.viewport.Width) {
            x = (int)(Game1.viewport.Width - textSize.X);
            y += Game1.tileSize / 4;
        }

        if (y + height > Game1.viewport.Height) {
            x += Game1.tileSize / 4;
            y = (int)(Game1.viewport.Height - height);
        }
        return new Rectangle(x, y, (int)textSize.X + Game1.tileSize / 2, (int)textSize.Y);
    }

    private static Rectangle ApplyTooltipIconPositions(Rectangle toolTipBounds, params Tooltip[] tooltips)
    {
        var result = new Rectangle(toolTipBounds.X, toolTipBounds.Y, toolTipBounds.Width, toolTipBounds.Height);

        foreach (var tooltip in tooltips) {
            var icons = tooltip.Icon;
            if (icons == null) {
                continue;
            }
            foreach (var icon in icons.Where(i => i != null)) {
                var iconPosition = icon!.CalculateTooltipPosition(result);
                result.X = Math.Min(result.X, iconPosition.X);
                result.Y = Math.Min(result.Y, iconPosition.Y);
                result.Width = Math.Max(result.X + result.Width, iconPosition.X + iconPosition.Width) - result.X;
                result.Height = Math.Max(result.Y + result.Height, iconPosition.Y + iconPosition.Height) - result.Y;
                break;
            }
        }
        // TODO: test this method?
        return result;
    }

    private static void DrawSimpleTooltip(SpriteBatch b, Tooltip tooltip, SpriteFont font, Rectangle textureBoxBounds, Vector2 textPosition)
    {
        IClickableMenu.drawTextureBox(b, Game1.menuTexture, TooltipSourceRect, textureBoxBounds.X, textureBoxBounds.Y,
            textureBoxBounds.Width, textureBoxBounds.Height, Color.White);

        var position = new Vector2(textPosition.X + Game1.tileSize / 4f, textPosition.Y + Game1.tileSize / 4f + 4);
        b.DrawString(font, tooltip.Text, position + new Vector2(2f, 2f), Game1.textShadowColor, 0, Vector2.Zero, 1f, SpriteEffects.None, 0);
        b.DrawString(font, tooltip.Text, position + new Vector2(0f, 2f), Game1.textShadowColor, 0, Vector2.Zero, 1f, SpriteEffects.None, 0);
        b.DrawString(font, tooltip.Text, position + new Vector2(2f, 0f), Game1.textShadowColor, 0, Vector2.Zero, 1f, SpriteEffects.None, 0);
        b.DrawString(font, tooltip.Text, position, Game1.textColor * 0.9f, 0, Vector2.Zero, 1f, SpriteEffects.None, 0);

        if (tooltip.Icon != null) {
            const int border = 3 * Game1.pixelZoom;
            var textureBoxBoundsWithoutBorder = new Rectangle {
                X = textureBoxBounds.X + border,
                Y = textureBoxBounds.Y + border,
                Width = textureBoxBounds.Width - 2 * border,
                Height = textureBoxBounds.Height - 2 * border,
            };

            var icons = tooltip.Icon
                .OfType<Icon>()
                .GroupBy(i => i.Position ?? IPosition.TopLeft);
            foreach (var subsets in icons) {
                var offset = new Point();
                var horizontalAlignment = IsHorizontalAligned(textureBoxBoundsWithoutBorder, subsets.First().CalculateIconPosition(textureBoxBoundsWithoutBorder));

                if (subsets.Count() > 1) {
                    // if multiple icons, force align from middle
                    var subsetPos = subsets.Select(i => i.CalculateIconPosition(textureBoxBoundsWithoutBorder));
                    var subsetWidth = subsetPos.Sum(i => i.Width);
                    var subsetHeight = subsetPos.Sum(i => i.Height);

                    offset = horizontalAlignment
                        ? new Point {
                            X = textureBoxBoundsWithoutBorder.X + (textureBoxBoundsWithoutBorder.Width - subsetWidth) / 2,
                            Y = (int)subsetPos.Average(i => i.Y)
                        }
                        : new Point {
                            X = (int)subsetPos.Average(i => i.X),
                            Y = textureBoxBoundsWithoutBorder.Y + (textureBoxBoundsWithoutBorder.Height - subsetHeight) / 2,
                        };
                }

                foreach (var icon in subsets) {
                    var iconPosition = icon.CalculateIconPosition(textureBoxBoundsWithoutBorder);

                    if (subsets.Count() > 1) {
                        iconPosition.Location = offset;
                        if (horizontalAlignment) {
                            offset.X += iconPosition.Width;
                        } else {
                            offset.Y += iconPosition.Height;
                        }
                    }

                    b.Draw(
                        icon.Texture,
                        iconPosition,
                        icon.NullSafeSourceRectangle,
                        Color.White
                    );
                }
            }
        }
    }

    private static bool IsHorizontalAligned(Rectangle container, Rectangle element)
    {
        var diffX = Math.Abs(container.X - element.X);
        var diffX2 = Math.Abs(container.Width + container.X - element.Width - element.X);
        diffX = Math.Min(diffX, diffX2);
        var diffY = Math.Abs(container.Y - element.Y);
        var diffY2 = Math.Abs(container.Height + container.Y - element.Height - element.Y);
        diffY = Math.Min(diffY, diffY2);

        return diffX >= diffY;
    }

    public void Add(ITooltipGenerator<TerrainFeature> generator)
    {
        _terrainFeatureManager ??= new BaseTooltipGeneratorManager<TerrainFeature>();
        _terrainFeatureManager.Add(generator);
    }

    public void Add(ITooltipGenerator<SObject> generator)
    {
        _objectInformant ??= new BaseTooltipGeneratorManager<SObject>();
        _objectInformant.Add(generator);
    }

    public void Add(ITooltipGenerator<Character> generator)
    {
        _characterInformant ??= new BaseTooltipGeneratorManager<Character>();
        _characterInformant.Add(generator);
    }

    public void Remove(string generatorId)
    {
        _terrainFeatureManager?.Remove(generatorId);
        _objectInformant?.Remove(generatorId);
    }
}