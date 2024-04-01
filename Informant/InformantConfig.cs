using Slothsoft.Informant.Implementation.TooltipGenerator;

namespace Slothsoft.Informant;

internal record InformantConfig
{
    public Dictionary<string, bool> DisplayIds { get; set; } = [];
    public TooltipTrigger TooltipTrigger { get; set; } = TooltipTrigger.Hover;
    public SButton TooltipTriggerButton { get; set; } = SButton.MouseRight;
    public HideMachineTooltips HideMachineTooltips { get; set; } = HideMachineTooltips.ForNonMachines;
    public bool DecorateFertilizer { get; set; } = true;
    public bool DecoratePet { get; set; } = true;
    public bool DecorateLockedBundles { get; set; } = true;
    public bool DecorateUnqualifiedBundles { get; set; } = true;
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
