namespace Slothsoft.Informant.Api;

/// <summary>
///     A class that decorates a vanilla tooltip using <see cref="Decoration" />.
/// </summary>
/// <typeparam name="TInput">input object.</typeparam>
public interface IDecorator<in TInput> : IDisplayable
{
    /// <summary>
    ///     Returns true if <see cref="Decorate" /> should be called on this object.
    /// </summary>
    bool HasDecoration(TInput input);

    /// <summary>
    ///     Generates the decorator for this object.
    /// </summary>
    Decoration Decorate(TInput input);
}