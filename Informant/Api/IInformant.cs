using Microsoft.Xna.Framework.Graphics;
using Slothsoft.Informant.Implementation.TooltipGenerator;
using Slothsoft.Informant.Implementation;
using StardewModdingAPI;
using StardewValley.Characters;
using StardewValley.TerrainFeatures;

namespace Slothsoft.Informant.Api;

/// <summary>
/// Base class for the entire API. Can be used to add custom information providers.
/// </summary>
public interface IInformant
{

    /// <summary>
    /// A manager class for the <see cref="TerrainFeature"/>(s) under the mouse position.
    /// </summary>
    ITooltipGeneratorManager<TerrainFeature> TerrainFeatureTooltipGenerators { get; }

    /// <summary>
    /// Adds a tooltip generator for the <see cref="TerrainFeature"/>(s) under the mouse position.
    /// <br/><b>Since Version:</b> 1.3.0
    /// </summary>
    void AddTerrainFeatureTooltipGenerator(string id, Func<string> displayName, Func<string> description, Func<TerrainFeature, string> generator);

    /// <summary>
    /// A manager class for the <see cref="SObject"/>(s) under the mouse position.
    /// </summary>
    ITooltipGeneratorManager<SObject> ObjectTooltipGenerators { get; }

    /// <summary>
    /// Adds a tooltip generator for the <see cref="SObject"/>(s) under the mouse position.
    /// <br/><b>Since Version:</b> 1.3.0
    /// </summary>
    void AddObjectTooltipGenerator(string id, Func<string> displayName, Func<string> description, Func<SObject, string?> generator);

    /// <summary>
    /// A manager class for the <see cref="FarmAnimal"/>(s) under the mouse position.
    /// <br/><b>Since Version:</b> 1.5.0
    /// </summary>
    ITooltipGeneratorManager<FarmAnimal> AnimalTooltipGenerator { get; }

    /// <summary>
    /// Adds a tooltip generator for the <see cref="FarmAnimal"/>(s) under the mouse position.
    /// <br/><b>Since Version:</b> 1.5.0
    /// </summary>
    void AddAnimalTooltipGenerator(string id, Func<string> displayName, Func<string> description, Func<FarmAnimal, string?> generator);

    /// <summary>
    /// Adds a tooltip generator for the <see cref="Pet"/>(s) under the mouse position.
    /// </summary>
    /// <br/><b>Since Version:</b> 1.7.0
    public ITooltipGeneratorManager<Pet> PetTooltipGenerator { get; }

    /// <summary>
    /// Adds a tooltip generator for the <see cref="Pet"/>(s) under the mouse position.
    /// </summary>
    /// <br/><b>Since Version:</b> 1.7.0
    void AddPetTooltipGenerator(string id, Func<string> displayName, Func<string> description, Func<Pet, string?> generator);

    /// <summary>
    /// A manager class for decorating a tooltip for an <see cref="Item"/>.
    /// </summary>
    IDecoratorManager<Item> ItemDecorators { get; }

    /// <summary>
    /// Adds a decorator for the <see cref="Item"/>(s) under the mouse position.
    /// <br/><b>Since Version:</b> 1.3.0
    /// </summary>
    void AddItemDecorator(string id, Func<string> displayName, Func<string> description, Func<Item, Texture2D?> decorator);

    /// <summary>
    /// A list of other classes that add information somwhere in the game.
    /// <br/><b>Since Version:</b> 1.2.1
    /// </summary>
    IEnumerable<IDisplayable> GeneralDisplayables { get; }
}