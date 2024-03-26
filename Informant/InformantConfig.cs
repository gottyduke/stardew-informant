namespace Slothsoft.Informant;

internal record InformantConfig
{
    public Dictionary<string, bool> DisplayIds { get; set; } = new();
    public TooltipTrigger TooltipTrigger { get; set; } = TooltipTrigger.Hover;
    public SButton TooltipTriggerButton { get; set; } = SButton.MouseRight;
    public HideMachineTooltips HideMachineTooltips { get; set; } = HideMachineTooltips.ForNonMachines;
    public bool DecorateLockedBundles { get; set; }
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
