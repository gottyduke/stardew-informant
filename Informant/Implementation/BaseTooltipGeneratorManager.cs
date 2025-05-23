﻿using System.Collections.Immutable;
using Slothsoft.Informant.Api;

namespace Slothsoft.Informant.Implementation;

internal class BaseTooltipGeneratorManager<TInput> : ITooltipGeneratorManager<TInput>
{
    private readonly List<ITooltipGenerator<TInput>> _generators = [];

    public IEnumerable<IDisplayable> Generators => _generators.ToImmutableArray();

    public void Add(ITooltipGenerator<TInput> generator)
    {
        _generators.Add(generator);
    }

    public void Remove(string generatorId)
    {
        _generators.RemoveAll(g => g.Id == generatorId);
    }

    internal IEnumerable<Tooltip> Generate(params TInput[] inputs)
    {
        var config = InformantMod.Instance?.Config ?? new InformantConfig();
        return _generators
            .Where(g => config.DisplayIds.GetValueOrDefault(g.Id, true))
            .SelectMany(g => inputs
                .OfType<TInput>()
                .Where(g.HasTooltip)
                .Select(g.Generate)
            );
    }
}