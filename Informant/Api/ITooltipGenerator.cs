namespace Slothsoft.Informant.Api;

/// <summary>
///     A class that generates a <see cref="Tooltip" />.
/// </summary>
/// <typeparam name="TInput">input object.</typeparam>
public interface ITooltipGenerator<in TInput> : IDisplayable
{
    /// <summary>
    ///     Returns true if <see cref="Generate" /> should be called on this object.
    /// </summary>
    bool HasTooltip(TInput input);

    /// <summary>
    ///     Generates the tooltip for this object.
    /// </summary>
    Tooltip Generate(TInput input);
}