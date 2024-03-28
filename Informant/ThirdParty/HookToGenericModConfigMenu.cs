using Slothsoft.Informant.Api;

namespace Slothsoft.Informant.ThirdParty;

internal static class HookToGenericModConfigMenu
{

    public static void Apply(InformantMod informantMod, IInformant api)
    {
        // get Generic Mod Config Menu's API (if it's installed)
        var configMenu = informantMod.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
        if (configMenu is null)
            return;

        // register mod
        configMenu.Register(
            mod: informantMod.ModManifest,
            reset: () => informantMod.Config = new InformantConfig(),
            save: () => informantMod.Helper.WriteConfig(informantMod.Config)
        );

        // add some config options for tooltip generators
        configMenu.AddSectionTitle(informantMod.ModManifest, () => informantMod.Helper.Translation.Get("Config.TooltipGenerators.GeneralSection"));
        configMenu.AddEnumOption(
            mod: informantMod.ModManifest,
            name: () => informantMod.Helper.Translation.Get("Config.TooltipTrigger"),
            getValue: () => informantMod.Config.TooltipTrigger,
            setValue: value => informantMod.Config.TooltipTrigger = value,
            getDisplayName: value => informantMod.Helper.Translation.Get("Config.TooltipTrigger." + value)
        );
        configMenu.AddKeybind(
            mod: informantMod.ModManifest,
            name: () => informantMod.Helper.Translation.Get("Config.TooltipTriggerButton"),
            getValue: () => informantMod.Config.TooltipTriggerButton,
            setValue: value => informantMod.Config.TooltipTriggerButton = value
        );
        configMenu.AddEnumOption(
            mod: informantMod.ModManifest,
            name: () => informantMod.Helper.Translation.Get("Config.HideMachineTooltips"),
            getValue: () => informantMod.Config.HideMachineTooltips,
            setValue: value => informantMod.Config.HideMachineTooltips = value,
            getDisplayName: value => informantMod.Helper.Translation.Get("Config.HideMachineTooltips." + value)
        );

        configMenu.AddSectionTitle(informantMod.ModManifest, () => informantMod.Helper.Translation.Get("Config.TooltipGenerators.Visibility"));
        var configurables = new List<IDisplayable>();
        configurables = [
            .. api.ObjectTooltipGenerators.Generators,
            .. api.TerrainFeatureTooltipGenerators.Generators,
            .. api.AnimalTooltipGenerator.Generators
        ];
        CreateDisplayableOptions(configMenu, configurables, informantMod);
        configMenu.AddBoolOption(
            mod: informantMod.ModManifest,
            name: () => informantMod.Helper.Translation.Get("Config.ShowCropFertilizer"),
            tooltip: () => informantMod.Helper.Translation.Get("Config.ShowCropFertilizer.Description"),
            getValue: () => informantMod.Config.ShowCropFertilizer,
            setValue: value => informantMod.Config.ShowCropFertilizer = value
        );

        // add some config options for decorators
        configMenu.AddSectionTitle(informantMod.ModManifest, () => informantMod.Helper.Translation.Get("Config.Decorators.GeneralSection"));
        configMenu.AddBoolOption(
            mod: informantMod.ModManifest,
            name: () => informantMod.Helper.Translation.Get("Config.DecorateLockedBundles"),
            tooltip: () => informantMod.Helper.Translation.Get("Config.DecorateLockedBundles.Description"),
            getValue: () => informantMod.Config.DecorateLockedBundles,
            setValue: value => informantMod.Config.DecorateLockedBundles = value
        );

        configMenu.AddSectionTitle(informantMod.ModManifest, () => informantMod.Helper.Translation.Get("Config.Decorators.Visibility"));
        configurables = [
            .. api.ItemDecorators.Decorators,
            .. api.GeneralDisplayables
        ];
        CreateDisplayableOptions(configMenu, configurables, informantMod);
    }

    private static void AddEnumOption<TEnum>(this IGenericModConfigMenuApi configMenu, IManifest mod, Func<TEnum> getValue, Action<TEnum> setValue,
        Func<string> name, Func<TEnum, string> getDisplayName) where TEnum : notnull
    {
        var enumValues = Enum.GetValues(typeof(TEnum)).Cast<TEnum>().ToArray();
        var enumStrings = enumValues.Select(e => e.ToString()!).ToArray();

        configMenu.AddTextOption(
            mod: mod,
            name: name,
            getValue: () => getValue().ToString()!,
            setValue: value => setValue(enumValues[Array.IndexOf(enumStrings, value)]),
            allowedValues: enumStrings,
            formatAllowedValue: value => getDisplayName(enumValues[Array.IndexOf(enumStrings, value)])
        );
    }

    private static void CreateDisplayableOptions(IGenericModConfigMenuApi configMenu, IEnumerable<IDisplayable> configurables, InformantMod informantMod)
    {
        foreach (var configurable in configurables.OrderBy(d => d.DisplayName)) {
            configMenu.AddBoolOption(
                mod: informantMod.ModManifest,
                name: () => configurable.DisplayName,
                tooltip: () => configurable.Description,
                getValue: () => informantMod.Config.DisplayIds.GetValueOrDefault(configurable.Id, true),
                setValue: value => informantMod.Config.DisplayIds[configurable.Id] = value
            );
        }
    }
}