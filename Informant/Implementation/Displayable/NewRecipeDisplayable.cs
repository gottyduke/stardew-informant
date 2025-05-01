using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using Slothsoft.Informant.Api;
using StardewValley.Menus;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Slothsoft.Informant.Implementation.Displayable;

internal class NewRecipeDisplayable : IDisplayable
{
    private static readonly Rectangle NewSourceRectangle = new(141, 438, 20, 9);
    private static Texture2D? cursor;

    private static Dictionary<ClickableTextureComponent, CraftingRecipe>? _componentToRecipe;
    private readonly Harmony _harmony;

    private readonly IModHelper _modHelper;

    public NewRecipeDisplayable(IModHelper modHelper, string? uniqueId = null)
    {
        cursor ??= Game1.content.Load<Texture2D>(Game1.mouseCursorsName);

        _modHelper = modHelper;
        _harmony = new(uniqueId ?? InformantMod.Instance!.ModManifest.UniqueID);
        _harmony.Patch(
            AccessTools.Method(
                typeof(ClickableTextureComponent),
                nameof(ClickableTextureComponent.draw),
                [
                    typeof(SpriteBatch),
                    typeof(Color),
                    typeof(float),
                    typeof(int),
                    typeof(int),
                    typeof(int),
                ]
            ),
            postfix: new(typeof(NewRecipeDisplayable), nameof(DrawOverlayIfNecessary))
        );

        _harmony.Patch(
            AccessTools.Method(
                typeof(CraftingPage),
                nameof(CraftingPage.draw),
                [
                    typeof(SpriteBatch),
                ]
            ),
            new(typeof(NewRecipeDisplayable), nameof(BeforeCraftingPageDraw)),
            new(typeof(NewRecipeDisplayable), nameof(AfterCraftingPageDraw))
        );
    }

    private static string DisplayableId => "new-recipe";

    public string Id => DisplayableId;
    public string DisplayName => _modHelper.Translation.Get("NewRecipeDisplayable");
    public string Description => _modHelper.Translation.Get("NewRecipeDisplayable.Description");

    private static void DrawOverlayIfNecessary(ClickableTextureComponent __instance, SpriteBatch b)
    {
        var recipe = _componentToRecipe?.GetValueOrDefault(__instance);
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
            ? Game1.player.recipesCooked.ContainsKey(recipe.getIndexOfMenuView())
                ? Game1.player.recipesCooked[recipe.getIndexOfMenuView()]
                : 0
            : Game1.player.craftingRecipes.ContainsKey(recipe.name)
                ? Game1.player.craftingRecipes[recipe.name]
                : 0;
        if (timesCrafted > 0) {
            // we are on the recipe page, have a recipe which was already craftet? nice, it's not new
            return;
        }

        var scale = recipe.isCookingRecipe ? 1.5f : 2.5f;
        b.Draw(cursor, __instance.bounds with {
            Width = (int)(NewSourceRectangle.Width * scale),
            Height = (int)(NewSourceRectangle.Height * scale),
        }, NewSourceRectangle, Color.White, 0, new(0, 0), SpriteEffects.None, 1);
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