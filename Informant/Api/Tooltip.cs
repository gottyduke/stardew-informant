namespace Slothsoft.Informant.Api;

/// <summary>
/// A class that contains all information to create a tooltip.
/// </summary>
/// <param name="Text">the multiline text to display.</param>
public record Tooltip(string Text)
{
    /// <summary>
    /// Optionally displays icon(s) around the tooltip.
    /// </summary>
    public Icon?[]? Icon { get; init; }
}