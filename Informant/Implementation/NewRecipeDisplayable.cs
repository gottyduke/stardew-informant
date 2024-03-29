using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Slothsoft.Informant.Api;
using StardewValley.Menus;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Slothsoft.Informant.Implementation;

internal class NewRecipeDisplayable : IDisplayable
{

    private static string DisplayableId => "new-recipe";
    private static readonly Rectangle NewSourceRectangle = new(141, 438, 20, 9);

    private readonly IModHelper _modHelper;
    private readonly Harmony _harmony;

    private static Dictionary<ClickableTextureComponent, CraftingRecipe>? _componentToRecipe;

    public NewRecipeDisplayable(IModHelper modHelper, string? uniqueId = null)
    {
        _modHelper = modHelper;
        _harmony = new Harmony(uniqueId ?? InformantMod.Instance!.ModManifest.UniqueID);
        _harmony.Patch(
            original: AccessTools.Method(
                typeof(ClickableTextureComponent),
                nameof(ClickableTextureComponent.draw),
                [
                    typeof(SpriteBatch),
                    typeof(Color),
                    typeof(float),
                    typeof(int),
                    typeof(int),
                ]
            ),
            postfix: new HarmonyMethod(typeof(NewRecipeDisplayable), nameof(DrawOverlayIfNecessary))
        );

        _harmony.Patch(
            original: AccessTools.Method(
                typeof(CraftingPage),
                nameof(CraftingPage.draw),
                [
                    typeof(SpriteBatch),
                ]
            ),
            prefix: new HarmonyMethod(typeof(NewRecipeDisplayable), nameof(BeforeCraftingPageDraw)),
            postfix: new HarmonyMethod(typeof(NewRecipeDisplayable), nameof(AfterCraftingPageDraw))
        );
    }

    public string Id => DisplayableId;
    public string DisplayName => _modHelper.Translation.Get("NewRecipeDisplayable");
    public string Description => _modHelper.Translation.Get("NewRecipeDisplayable.Description");

    private static void DrawOverlayIfNecessary(ClickableTextureComponent __instance, SpriteBatch b)
    {
        if (_componentToRecipe == null) {
            // this can be any kind of clickable in the game - we ignore everything that is no recipe
            return;
        }

        var recipe = _componentToRecipe.GetValueOrDefault(__instance);
        if (recipe == null) {
            // we are on the recipe page, but have no recipe? ignore!
            return;
        }

        var config = InformantMod.Instance?.Config ?? new InformantConfig();
        if (!config.DisplayIds.GetValueOrDefault(DisplayableId, true)) {
            return; // this "decorator" is deactivated
        }

        // recipe.timesCrafted is not updated it seems
        var timesCrafted = recipe.isCookingRecipe
                ? (Game1.MasterPlayer.recipesCooked.ContainsKey(recipe.getIndexOfMenuView()) ? Game1.MasterPlayer.recipesCooked[recipe.getIndexOfMenuView()] : 0)
                : (Game1.MasterPlayer.craftingRecipes.ContainsKey(recipe.name) ? Game1.MasterPlayer.craftingRecipes[recipe.name] : 0);
        if (timesCrafted > 0) {
            // we are on the recipe page, have a recipe which was already craftet? nice, it's not new
            return;
        }

        var scale = recipe.isCookingRecipe ? 1.5f : 2.5f;
        b.Draw(Game1.mouseCursors, __instance.bounds with {
            Width = (int)(NewSourceRectangle.Width * scale),
            Height = (int)(NewSourceRectangle.Height * scale),
        }, NewSourceRectangle, Color.White, 0, new Vector2(0, 0), SpriteEffects.None, 1);
    }


    private static void BeforeCraftingPageDraw(CraftingPage __instance)
    {
        _componentToRecipe = __instance.pagesOfCraftingRecipes
            .SelectMany(p => p)
            .ToDictionary(e => e.Key, e => e.Value);
    }

    private static void AfterCraftingPageDraw()
    {
        _componentToRecipe = null;
    }
}