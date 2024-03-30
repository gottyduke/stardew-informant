﻿using Microsoft.Xna.Framework;
using Slothsoft.Informant.Api;
using Slothsoft.Informant.Implementation.Common;
using StardewValley.Objects;

namespace Slothsoft.Informant.Implementation.TooltipGenerator;

internal class MachineTooltipGenerator : ITooltipGenerator<SObject>
{

    private readonly IModHelper _modHelper;

    public MachineTooltipGenerator(IModHelper modHelper)
    {
        _modHelper = modHelper;
    }

    public string Id => "machine";
    public string DisplayName => _modHelper.Translation.Get("MachineTooltipGenerator");
    public string Description => _modHelper.Translation.Get("MachineTooltipGenerator.Description");

    public bool HasTooltip(SObject input)
    {
        return HasTooltip(input, InformantMod.Instance?.Config.HideMachineTooltips ?? HideMachineTooltips.ForNonMachines);
    }

    internal static bool HasTooltip(SObject input, HideMachineTooltips hideMachineTooltips)
    {
        if (!input.bigCraftable.Value) return false;
        _ = int.TryParse(input.ItemId, out var unqualifiedItemId);
        if (unqualifiedItemId == BigCraftableIds.GardenPot) {
            var gardenPot = input as IndoorPot;
            var crop = gardenPot?.hoeDirt.Value.crop;
            return crop != null;
        }

        return hideMachineTooltips switch {
            HideMachineTooltips.Never => true,
            HideMachineTooltips.ForChests => !BigCraftableIds.AllChests.Contains(unqualifiedItemId),
            _ => !BigCraftableIds.AllChests.Contains(unqualifiedItemId) &&
                 !BigCraftableIds.AllStaticCraftables.Contains(unqualifiedItemId)
        };
    }

    public Tooltip Generate(SObject input)
    {
        _ = int.TryParse(input.ItemId, out var unqualifiedItemId);
        if (unqualifiedItemId == BigCraftableIds.GardenPot) {
            return input is not IndoorPot gardenPot || gardenPot.hoeDirt.Value.crop == null
                ? new Tooltip("???")
                : CropTooltipGenerator.CreateTooltip(_modHelper, gardenPot.hoeDirt.Value);
        }
        return CreateTooltip(input);
    }

    private Tooltip CreateTooltip(SObject input)
    {
        var displayName = input.DisplayName;
        _ = int.TryParse(input.ItemId, out var unqualifiedItemId);

        var heldObject = input.heldObject.Value;
        if (heldObject == null || BigCraftableIds.AutoGrabber == unqualifiedItemId) {
            return new Tooltip(displayName); // we don't show any icon for AutoGrabber
        }
        var heldObjectName = heldObject.DisplayName;
        var daysLeft = CalculateMinutesLeftString(input);
        return new Tooltip($"{displayName}\n> {heldObjectName}\n{daysLeft}") {
            Icon = [
                Icon.ForObject(
                    heldObject,
                    IPosition.CenterRight,
                    new Vector2(Game1.tileSize / 2, Game1.tileSize / 2)
                )
            ]
        };
    }

    internal string CalculateMinutesLeftString(SObject input)
    {
        if (input is Cask cask) {
            return CalculateMinutesLeftStringForCask(cask);
        }

        var minutesUntilReady = input.MinutesUntilReady;
        switch (minutesUntilReady) {
            case < 0:
                return _modHelper.Translation.Get("MachineTooltipGenerator.CannotBeUnloaded");
            case 0:
                return _modHelper.Translation.Get("MachineTooltipGenerator.Finished");
        }
        var minutesLeft = minutesUntilReady % 60;
        var hoursLeft = (minutesUntilReady / 60) % 24;
        var daysLeft = minutesUntilReady / 60 / 24;
        return $"{daysLeft:D2}:{hoursLeft:D2}:{minutesLeft:D2}";
    }

    private string CalculateMinutesLeftStringForCask(Cask input)
    {
        if (input.MinutesUntilReady == 1) {
            return _modHelper.Translation.Get("MachineTooltipGenerator.Finished");
        }
        var daysForQuality = input.GetDaysForQuality(input.GetNextQuality(input.heldObject.Value.Quality));
        var daysNeededForNextQuality = (int)((input.daysToMature.Value - daysForQuality) / input.agingRate.Value);
        var daysNeededTotal = (int)(input.daysToMature.Value / input.agingRate.Value);

        if (daysNeededTotal <= 0) {
            // if the wine is finished, we only need "Finished" once
            return _modHelper.Translation.Get("MachineTooltipGenerator.Finished");
        }

        var daysNeededForNextQualityString = _modHelper.Translation.Get("MachineTooltipGenerator.ForNextQuality",
            new { X = CropTooltipGenerator.ToDaysLeftString(_modHelper, daysNeededForNextQuality) });
        var daysNeededTotalString = _modHelper.Translation.Get("MachineTooltipGenerator.ForTotal",
            new { X = CropTooltipGenerator.ToDaysLeftString(_modHelper, daysNeededTotal) });
        return $"{daysNeededForNextQualityString}\n{daysNeededTotalString}";
    }
}