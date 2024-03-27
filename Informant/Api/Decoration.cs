using Microsoft.Xna.Framework.Graphics;

namespace Slothsoft.Informant.Api;

/// <summary>
/// A class that contains all information to decorate a vanilla tooltip.
/// </summary>
/// <param name="Texture">the texture to display.</param>
public record Decoration(Texture2D Texture)
{
    /// <summary>
    /// Optionally displays a little number over the texture.
    /// </summary>
    public int? Counter { get; init; }

    /// <summary>
    /// Optional extra decorations of the same decorator.
    /// </summary>
    public Decoration[]? ExtraDecorations { get; init; }
}