using Microsoft.Xna.Framework.Graphics;

// ReSharper disable once CheckNamespace
namespace Slothsoft.Informant.Api;

/// <summary>
///     Base class for the entire API. Can be used to add custom information providers.
/// </summary>
public interface IInformant
{
    /// <summary>
    ///     Adds a decorator for the <see cref="Item" />(s) under the mouse position.
    ///     <br /><b>Since Version:</b> 1.3.0
    /// </summary>
    void AddItemDecorator(string id, Func<string> displayName, Func<string> description, Func<Item, Texture2D?> decorator);
}