using Microsoft.Xna.Framework.Graphics;
using Slothsoft.Informant.Api;
using Slothsoft.Informant.Implementation.Decorator;
using Slothsoft.Informant.Implementation.Displayable;
using Slothsoft.Informant.Implementation.TooltipGenerator;
using StardewValley.TerrainFeatures;

namespace Slothsoft.Informant.Implementation;

public class Informant(IModHelper modHelper) : IInformant
{
    private TooltipGeneratorManager? _tooltipGeneratorManager;
    private ItemDecoratorManager? _itemDecoratorInformant;
    private readonly SellPriceDisplayable _sellPriceDisplayable = new(modHelper);
    private readonly NewRecipeDisplayable _newRecipeDisplayable = new(modHelper);

    public ITooltipGeneratorManager<TerrainFeature> TerrainFeatureTooltipGenerators
    {
        get
        {
            _tooltipGeneratorManager ??= new TooltipGeneratorManager(modHelper);
            return _tooltipGeneratorManager;
        }
    }

    public void AddTerrainFeatureTooltipGenerator(string id, Func<string> displayName, Func<string> description, Func<TerrainFeature, string> generator)
    {
        TerrainFeatureTooltipGenerators.Add(new TooltipGenerator<TerrainFeature>(id, displayName, description, generator));
    }

    public ITooltipGeneratorManager<SObject> ObjectTooltipGenerators
    {
        get
        {
            _tooltipGeneratorManager ??= new TooltipGeneratorManager(modHelper);
            return _tooltipGeneratorManager;
        }
    }

    public void AddObjectTooltipGenerator(string id, Func<string> displayName, Func<string> description, Func<SObject, string?> generator)
    {
        ObjectTooltipGenerators.Add(new TooltipGenerator<SObject>(id, displayName, description, generator));
    }

    public ITooltipGeneratorManager<Character> CharacterTooltipGenerators
    {
        get
        {
            _tooltipGeneratorManager ??= new TooltipGeneratorManager(modHelper);
            return _tooltipGeneratorManager;
        }
    }

    public void AddCharacterTooltipGenerator(string id, Func<string> displayName, Func<string> description, Func<Character, string?> generator)
    {
        CharacterTooltipGenerators.Add(new TooltipGenerator<Character>(id, displayName, description, generator));
    }

    public IDecoratorManager<Item> ItemDecorators
    {
        get
        {
            _itemDecoratorInformant ??= new ItemDecoratorManager(modHelper);
            return _itemDecoratorInformant;
        }
    }

    public void AddItemDecorator(string id, Func<string> displayName, Func<string> description, Func<Item, Texture2D?> decorator)
    {
        ItemDecorators.Add(new Decorator<Item>(id, displayName, description, decorator));
    }

    public IEnumerable<IDisplayable> GeneralDisplayables => [_sellPriceDisplayable, _newRecipeDisplayable];
}