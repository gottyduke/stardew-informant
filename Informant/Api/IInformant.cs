using Microsoft.Xna.Framework.Graphics;
using StardewValley.TerrainFeatures;

namespace Slothsoft.Informant.Api;

/// <summary>
///     Base class for the entire API. Can be used to add custom information providers.
/// </summary>
public interface IInformant
{
    /// <summary>
    ///     A manager class for the <see cref="TerrainFeature" />(s) under the mouse position.
    /// </summary>
    ITooltipGeneratorManager<TerrainFeature> TerrainFeatureTooltipGenerators { get; }

    /// <summary>
    ///     A manager class for the <see cref="SObject" />(s) under the mouse position.
    /// </summary>
    ITooltipGeneratorManager<SObject> ObjectTooltipGenerators { get; }

    /// <summary>
    ///     A manager class for the <see cref="Character" />(s) under the mouse position.
    ///     <br /><b>Since Version:</b> 1.7.2
    /// </summary>
    ITooltipGeneratorManager<Character> CharacterTooltipGenerators { get; }

    /// <summary>
    ///     A manager class for decorating a tooltip for an <see cref="Item" />.
    /// </summary>
    IDecoratorManager<Item> ItemDecorators { get; }

    /// <summary>
    ///     A list of other classes that add information somwhere in the game.
    ///     <br /><b>Since Version:</b> 1.2.1
    /// </summary>
    IEnumerable<IDisplayable> GeneralDisplayables { get; }

    /// <summary>
    ///     Adds a tooltip generator for the <see cref="TerrainFeature" />(s) under the mouse position.
    ///     <br /><b>Since Version:</b> 1.3.0
    /// </summary>
    void AddTerrainFeatureTooltipGenerator(string id, Func<string> displayName, Func<string> description,
        Func<TerrainFeature, string> generator);

    /// <summary>
    ///     Adds a tooltip generator for the <see cref="SObject" />(s) under the mouse position.
    ///     <br /><b>Since Version:</b> 1.3.0
    /// </summary>
    void AddObjectTooltipGenerator(string id, Func<string> displayName, Func<string> description,
        Func<SObject, string?> generator);

    /// <summary>
    ///     Adds a tooltip generator for the <see cref="Character" />(s) under the mouse position.
    ///     <br /><b>Since Version:</b> 1.7.2
    /// </summary>
    void AddCharacterTooltipGenerator(string id, Func<string> displayName, Func<string> description,
        Func<Character, string?> generator);

    /// <summary>
    ///     Adds a decorator for the <see cref="Item" />(s) under the mouse position.
    ///     <br /><b>Since Version:</b> 1.3.0
    /// </summary>
    void AddItemDecorator(string id, Func<string> displayName, Func<string> description, Func<Item, Texture2D?> decorator);
}