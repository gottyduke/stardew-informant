using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Slothsoft.Informant.Api;

/// <summary>
///     All information needed to display an icon somewhere.
/// </summary>
/// <param name="Texture">the texture to display.</param>
public record Icon(Texture2D Texture)
{
    /// <summary>
    ///     Optionally defines the source rectangle of the texture. Will be the entire <see cref="Texture" /> if not set.
    /// </summary>
    public Rectangle? SourceRectangle { get; init; }

    /// <summary>
    ///     Optionally defines the position of the icon. Will be <see cref="IPosition.TopLeft" /> if not set.
    /// </summary>
    public IPosition? Position { get; init; }

    /// <summary>
    ///     Optionally defines the size of the icon. Will be the <see cref="Texture" />'s size if not set.
    /// </summary>
    public Vector2? IconSize { get; init; }

    internal Rectangle NullSafeSourceRectangle => SourceRectangle ?? new Rectangle(0, 0, Texture.Width, Texture.Height);
    private IPosition NullSafePosition => Position ?? IPosition.TopLeft;
    private Vector2 NullSafeIconSize => IconSize ?? new Vector2(NullSafeSourceRectangle.Width, NullSafeSourceRectangle.Height);

    /// <summary>
    ///     Create an icon for an Stardew Valley <see cref="SObject" />.
    /// </summary>
    public static Icon? ForObject(SObject obj, IPosition? position = null, Vector2? iconSize = null)
    {
        return ForUnqualifiedItemId(obj.QualifiedItemId, position, iconSize);
    }

    /// <summary>
    ///     Create an icon for a parentSheetIndex and bigCraftable.
    /// </summary>
    public static Icon? ForUnqualifiedItemId(string qualified, IPosition? position = null, Vector2? iconSize = null)
    {
        var item = ItemRegistry.GetDataOrErrorItem(qualified);
        position ??= IPosition.TopRight;
        iconSize ??= new(Game1.tileSize, Game1.tileSize);

        return new(item.GetTexture()) {
            SourceRectangle = item.GetSourceRect(),
            Position = position,
            IconSize = iconSize,
        };
    }

    internal Rectangle CalculateIconPosition(Rectangle tooltipBounds)
    {
        return NullSafePosition.CalculateIconPosition(tooltipBounds, NullSafeIconSize);
    }

    internal Rectangle CalculateTooltipPosition(Rectangle tooltipBounds)
    {
        return NullSafePosition.CalculateTooltipPosition(tooltipBounds, NullSafeIconSize);
    }
}