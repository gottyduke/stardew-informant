using Microsoft.Xna.Framework.Graphics;

namespace Informant.Aquarium;

public class AquariumDecorator
{
    private readonly Texture2D? _fish;
    private readonly IModHelper _modHelper;

    public AquariumDecorator(IModHelper modHelper)
    {
        _modHelper = modHelper;
        _fish ??= modHelper.ModContent.Load<Texture2D>("assets/aquarium.png");
    }
        
    public string Id => "aquarium";

    public string GetDisplayName()
    {
        return _modHelper.Translation.Get("AquariumTooltipDecorator");
    }
        
    public string GetDescription()
    {
        return _modHelper.Translation.Get("AquariumTooltipDecorator.Description");
    }

    public Texture2D? GetDecorator(Item input)
    {
        if (_fish == null || input.Category != SObject.FishCategory) return null;
            
        // Aquarium Data
        // ==========
        // See https://www.nexusmods.com/stardewvalley/mods/6372?tab=description
        //      (Section "Extra"->"Modder's reference")
        // "Mail flags are added to the Host player for each fish donated,
        //      with any spaces removed from the Fish name, for example:
        //      AquariumDonated:RainbowTrout."
        var mailFlag = $"AquariumDonated:{input.Name.Replace(" ", string.Empty)}";
        return !Game1.MasterPlayer.hasOrWillReceiveMail(mailFlag) ? _fish : null;
    }
}