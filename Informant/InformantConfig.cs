namespace Slothsoft.Informant;

internal record InformantConfig
{
    public Dictionary<string, bool> DisplayIds { get; set; } = [];
    public TooltipTrigger TooltipTrigger { get; set; } = TooltipTrigger.Hover;
    public SButton TooltipTriggerButton { get; set; } = SButton.MouseRight;
    public HideMachineTooltips HideMachineTooltips { get; set; } = HideMachineTooltips.ForNonMachines;
    public ShippingBinTrackingType ShippingBinTracking { get; set; } = ShippingBinTrackingType.Collection;
    public bool DecorateNotWatered { get; set; } = true;
    public bool DecorateFertilizer { get; set; } = true;
    public bool DecorateTreeFertilizer { get; set; } = true;
    public bool DecoratePet { get; set; } = true;
    public bool DecorateLockedBundles { get; set; } = true;
    public bool DecorateUnqualifiedBundles { get; set; } = true;
    public bool ShowTreeGrowthStage { get; set; } = false;
}

internal enum TooltipTrigger
{
    Hover,
    ButtonHeld,
}

internal enum HideMachineTooltips
{
    ForNonMachines,
    ForChests,
    Never,
}

internal enum ShippingBinTrackingType
{
    All,
    Collection,
}