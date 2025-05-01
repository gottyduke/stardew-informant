using Slothsoft.Informant.Api;

namespace Informant.Aquarium;

public class InformantAquariumMod : Mod
{
    public override void Entry(IModHelper helper)
    {
        Helper.Events.GameLoop.GameLaunched += (_, _) => {
            var informant = Helper.ModRegistry.GetApi<IInformant>("Slothsoft.Informant");
            if (informant is null) {
                return;
            }

            var decorator = new AquariumDecorator(helper);
            informant.AddItemDecorator(decorator.Id, decorator.GetDisplayName, decorator.GetDescription, decorator.GetDecorator);
        };
    }
}