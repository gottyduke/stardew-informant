using Slothsoft.Informant.ThirdParty;

namespace Slothsoft.Informant.Api;

/// <summary>
///     A class that can be registered in the <see cref="IGenericModConfigMenuApi" />.
/// </summary>
public interface IDisplayable
{
    /// <summary>
    ///     The unique ID of the generator.
    /// </summary>
    string Id { get; }

    /// <summary>
    ///     The display name of the generator for the configuration.
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    ///     The description of the generator for the configuration.
    /// </summary>
    string Description { get; }
}