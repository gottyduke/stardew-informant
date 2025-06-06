﻿using Microsoft.Xna.Framework.Graphics;
using Slothsoft.Informant.Api;

namespace Slothsoft.Informant.Implementation.Decorator;

internal class MuseumDecorator : IDecorator<Item>
{
    private static readonly string[] ValidTypes = ["Arch", "Minerals"];

    private static Texture2D? _museum;

    private readonly IModHelper _modHelper;

    public MuseumDecorator(IModHelper modHelper)
    {
        _modHelper = modHelper;
        _museum ??= modHelper.ModContent.Load<Texture2D>("assets/museum.png");
    }

    public string Id => "museum";
    public string DisplayName => _modHelper.Translation.Get("MuseumDecorator");
    public string Description => _modHelper.Translation.Get("MuseumDecorator.Description");

    public bool HasDecoration(Item input)
    {
        if (_museum != null && input is SObject obj && !obj.bigCraftable.Value && obj.Type != null &&
            ValidTypes.Contains(obj.Type)) {
            return IsNeeded(obj);
        }

        return false;
    }

    public Decoration Decorate(Item input)
    {
        return new(_museum!);
    }

    private static bool IsNeeded(Item item)
    {
        return Game1.netWorldState.Value.MuseumPieces.Pairs
            .All(pair => item.ItemId != pair.Value);
    }
}