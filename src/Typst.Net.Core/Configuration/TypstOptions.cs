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
    /// The name of the environment variable for the Typst executable path.
    /// </summary>
    public const string ExecutablePathEnvVar = "TYPST_EXECUTABLE_PATH";

    /// <summary>
    /// Gets or sets the file path to the Typst executable.
    /// If not set, it will try to get the value from the environment variable TYPST_EXECUTABLE_PATH.
    /// </summary>
    public string ExecutablePath { get; set; } = Environment.GetEnvironmentVariable(ExecutablePathEnvVar) ?? string.Empty;
}