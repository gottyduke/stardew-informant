using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;

namespace Slothsoft.Informant.Api;

/// <summary>
///     A class that contains all information to decorate a vanilla tooltip.
/// </summary>
/// <param name="Texture">the texture to display.</param>
public record Decoration(Texture2D Texture)
{
    /// <summary>
    ///     Optionally displays a little number over the texture.
    /// </summary>
    public int? Counter { get; init; }

    /// <summary>
    ///     Color for the counter number.
    /// </summary>
    public Color? CounterColor { get; init; }

    /// <summary>
    ///     Optional extra decorations of the same decorator.
    /// </summary>
    /// <remarks>
    ///     This array is only flattened once;
    ///     if any extra decoration also contains this field, it will be ignored.
    /// </remarks>
    public Decoration[]? ExtraDecorations { get; init; }
}