using Slothsoft.Informant.Api;

namespace Slothsoft.Informant.Implementation.TooltipGenerator;

internal class TooltipGenerator<TInput> : ITooltipGenerator<TInput>
{
    private readonly Func<string> _description;

    private readonly Func<string> _displayName;
    private readonly Func<TInput, string?> _generator;

    public TooltipGenerator(string id, Func<string> displayName, Func<string> description, Func<TInput, string?> generator)
    {
        Id = id;
        _displayName = displayName;
        _description = description;
        _generator = generator;
    }

    public string Id { get; }
    public string DisplayName => _displayName();
    public string Description => _description();

    public bool HasTooltip(TInput input)
    {
        return _generator(input) != null;
    }

    public Tooltip Generate(TInput input)
    {
        return new(_generator(input)!);
    }
}