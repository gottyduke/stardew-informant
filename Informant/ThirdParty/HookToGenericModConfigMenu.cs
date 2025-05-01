using Slothsoft.Informant.Api;

namespace Slothsoft.Informant.ThirdParty;

internal static class HookToGenericModConfigMenu
{
    public static void Apply(InformantMod informantMod, IInformant api)
    {
        // get Generic Mod Config Menu's API (if it's installed)
        var configMenu = informantMod.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
        if (configMenu == null) {
            return;
        }

        // register mod
        configMenu.Register(
            informantMod.ModManifest,
            () => informantMod.Config = new(),
            () => informantMod.Helper.WriteConfig(informantMod.Config)
        );

        // add some config options for tooltip generators
        configMenu.AddSectionTitle(informantMod.ModManifest,
            () => informantMod.Helper.Translation.Get("Config.TooltipGenerators.GeneralSection"));
        configMenu.AddEnumOption(
            informantMod.ModManifest,
            name: () => informantMod.Helper.Translation.Get("Config.TooltipTrigger"),
            getValue: () => informantMod.Config.TooltipTrigger,
            setValue: value => informantMod.Config.TooltipTrigger = value,
            getDisplayName: value => informantMod.Helper.Translation.Get("Config.TooltipTrigger." + value)
        );
        configMenu.AddKeybind(
            informantMod.ModManifest,
            name: () => informantMod.Helper.Translation.Get("Config.TooltipTriggerButton"),
            getValue: () => informantMod.Config.TooltipTriggerButton,
            setValue: value => informantMod.Config.TooltipTriggerButton = value
        );
        configMenu.AddEnumOption(
            informantMod.ModManifest,
            name: () => informantMod.Helper.Translation.Get("Config.HideMachineTooltips"),
            getValue: () => informantMod.Config.HideMachineTooltips,
            setValue: value => informantMod.Config.HideMachineTooltips = value,
            getDisplayName: value => informantMod.Helper.Translation.Get("Config.HideMachineTooltips." + value)
        );

        // tooltips
        configMenu.AddSectionTitle(informantMod.ModManifest,
            () => informantMod.Helper.Translation.Get("Config.TooltipGenerators.Visibility"));
        List<IDisplayable> configurables = [
            .. api.ObjectTooltipGenerators.Generators,
            .. api.TerrainFeatureTooltipGenerators.Generators,
            .. api.CharacterTooltipGenerators.Generators,
        ];
        CreateDisplayableOptions(configMenu, configurables, informantMod);
        configMenu.AddSectionTitle(informantMod.ModManifest,
            () => informantMod.Helper.Translation.Get("Config.TooltipGenerators.GeneralSection"));
        configMenu.AddBoolOption(
            informantMod.ModManifest,
            name: () => informantMod.Helper.Translation.Get("CropTooltipGenerator.DecorateNotWatered"),
            tooltip: () => informantMod.Helper.Translation.Get("CropTooltipGenerator.DecorateNotWatered.Description"),
            getValue: () => informantMod.Config.DecorateNotWatered,
            setValue: value => informantMod.Config.DecorateNotWatered = value
        );
        configMenu.AddBoolOption(
            informantMod.ModManifest,
            name: () => informantMod.Helper.Translation.Get("CropTooltipGenerator.DecorateFertilizer"),
            tooltip: () => informantMod.Helper.Translation.Get("CropTooltipGenerator.DecorateFertilizer.Description"),
            getValue: () => informantMod.Config.DecorateFertilizer,
            setValue: value => informantMod.Config.DecorateFertilizer = value
        );
        configMenu.AddBoolOption(
            informantMod.ModManifest,
            name: () => informantMod.Helper.Translation.Get("TreeTooltipGenerator.DecorateFertilizer"),
            tooltip: () => informantMod.Helper.Translation.Get("TreeTooltipGenerator.DecorateFertilizer.Description"),
            getValue: () => informantMod.Config.DecorateTreeFertilizer,
            setValue: value => informantMod.Config.DecorateTreeFertilizer = value
        );
        configMenu.AddBoolOption(
            informantMod.ModManifest,
            name: () => informantMod.Helper.Translation.Get("AnimalTooltipGenerator.DecoratePet"),
            tooltip: () => informantMod.Helper.Translation.Get("AnimalTooltipGenerator.DecoratePet.Description"),
            getValue: () => informantMod.Config.DecoratePet,
            setValue: value => informantMod.Config.DecoratePet = value
        );
        configMenu.AddBoolOption(
            informantMod.ModManifest,
            name: () => informantMod.Helper.Translation.Get("TreeTooltipGenerator.ShowGrowthStage"),
            tooltip: () => informantMod.Helper.Translation.Get("TreeTooltipGenerator.ShowGrowthStage.Description"),
            getValue: () => informantMod.Config.ShowTreeGrowthStage,
            setValue: value => informantMod.Config.ShowTreeGrowthStage = value
        );

        // decorators
        configMenu.AddSectionTitle(informantMod.ModManifest,
            () => informantMod.Helper.Translation.Get("Config.Decorators.Visibility"));
        configurables = [
            .. api.ItemDecorators.Decorators,
            .. api.GeneralDisplayables,
        ];
        CreateDisplayableOptions(configMenu, configurables, informantMod);
        configMenu.AddSectionTitle(informantMod.ModManifest,
            () => informantMod.Helper.Translation.Get("Config.Decorators.GeneralSection"));
        configMenu.AddBoolOption(
            informantMod.ModManifest,
            name: () => informantMod.Helper.Translation.Get("BundleTooltipDecorator.DecorateLockedBundles"),
            tooltip: () => informantMod.Helper.Translation.Get("BundleTooltipDecorator.DecorateLockedBundles.Description"),
            getValue: () => informantMod.Config.DecorateLockedBundles,
            setValue: value => informantMod.Config.DecorateLockedBundles = value
        );
        configMenu.AddBoolOption(
            informantMod.ModManifest,
            name: () => informantMod.Helper.Translation.Get("BundleTooltipDecorator.DecorateUnqualifiedBundles"),
            tooltip: () => informantMod.Helper.Translation.Get("BundleTooltipDecorator.DecorateUnqualifiedBundles.Description"),
            getValue: () => informantMod.Config.DecorateUnqualifiedBundles,
            setValue: value => informantMod.Config.DecorateUnqualifiedBundles = value
        );
        configMenu.AddEnumOption(
            informantMod.ModManifest,
            name: () => informantMod.Helper.Translation.Get("ShippingBinDecorator.TrackingType"),
            getValue: () => informantMod.Config.ShippingBinTracking,
            setValue: value => informantMod.Config.ShippingBinTracking = value,
            getDisplayName: value => informantMod.Helper.Translation.Get("ShippingBinDecorator.TrackingType." + value)
        );
    }

    private static void AddEnumOption<TEnum>(this IGenericModConfigMenuApi configMenu, IManifest mod, Func<TEnum> getValue,
        Action<TEnum> setValue,
        Func<string> name, Func<TEnum, string> getDisplayName) where TEnum : notnull
    {
        var enumValues = Enum.GetValues(typeof(TEnum)).Cast<TEnum>().ToArray();
        var enumStrings = enumValues.Select(e => e.ToString()!).ToArray();

        configMenu.AddTextOption(
            mod,
            name: name,
            getValue: () => getValue().ToString()!,
            setValue: value => setValue(enumValues[Array.IndexOf(enumStrings, value)]),
            allowedValues: enumStrings,
            formatAllowedValue: value => getDisplayName(enumValues[Array.IndexOf(enumStrings, value)])
        );
    }

    private static void CreateDisplayableOptions(IGenericModConfigMenuApi configMenu, IEnumerable<IDisplayable> configurables,
        InformantMod informantMod)
    {
        foreach (var configurable in configurables.OrderBy(d => d.DisplayName)) {
            configMenu.AddBoolOption(
                informantMod.ModManifest,
                name: () => configurable.DisplayName,
                tooltip: () => configurable.Description,
                getValue: () => informantMod.Config.DisplayIds.GetValueOrDefault(configurable.Id, true),
                setValue: value => informantMod.Config.DisplayIds[configurable.Id] = value
            );
        }
    }
}