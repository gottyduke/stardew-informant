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

    public Tooltip Generate(SObject input)
    {
        if (input.ItemId == BigCraftableIds.GardenPot) {
            return input is not IndoorPot gardenPot || gardenPot.hoeDirt.Value.crop == null
                ? new("???")
                : CropTooltipGenerator.CreateTooltip(_modHelper, gardenPot.hoeDirt.Value);
        }

        return CreateTooltip(input);
    }

    internal static bool HasTooltip(SObject input, HideMachineTooltips hideMachineTooltips)
    {
        if (!input.bigCraftable.Value) {
            return false;
        }

        if (input.ItemId != BigCraftableIds.GardenPot) {
            return hideMachineTooltips switch {
                HideMachineTooltips.Never => true,
                HideMachineTooltips.ForChests => !BigCraftableIds.AllChests.Contains(input.ItemId),
                _ => !BigCraftableIds.AllChests.Contains(input.ItemId) &&
                     !BigCraftableIds.AllStaticCraftables.Contains(input.ItemId),
            };
        }

        var gardenPot = input as IndoorPot;
        var crop = gardenPot?.hoeDirt.Value.crop;
        return crop != null;
    }

    private Tooltip CreateTooltip(SObject input)
    {
        var displayName = input.DisplayName;

        var heldObject = input.heldObject.Value;
        if (heldObject == null || BigCraftableIds.AutoGrabber == input.ItemId) {
            return new(displayName); // we don't show any icon for AutoGrabber
        }

        var heldObjectName = heldObject.DisplayName;
        var heldObjectStack = heldObject.Stack > 1 ? $"x{heldObject.Stack}" : "";
        var daysLeft = CalculateMinutesLeftString(input);
        return new($"{displayName}\n> {heldObjectName} {heldObjectStack}\n{daysLeft}") {
            Icon = [
                Icon.ForObject(
                    heldObject,
                    IPosition.CenterRight,
                    new Vector2(Game1.tileSize / 2, Game1.tileSize / 2)
                ),
            ],
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
        var hoursLeft = minutesUntilReady / 60 % 24;
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