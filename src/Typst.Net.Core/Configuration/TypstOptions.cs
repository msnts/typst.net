namespace Typst.Net.Core.Configuration;

/// <summary>
/// Represents the configuration options for Typst.
/// </summary>
public class TypstOptions
{
    /// <summary>
    /// The name of the configuration section for Typst.
    /// </summary>
    public const string SectionName = "Typst";

    /// <summary>
    /// Gets or sets the file path to the Typst executable.
    /// </summary>
    public string ExecutablePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the default arguments to be used with the Typst executable.
    /// This value is optional and can be null.
    /// </summary>
    public string? DefaultArguments { get; set; }
}