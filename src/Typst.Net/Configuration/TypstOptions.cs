using System.Diagnostics.CodeAnalysis;

namespace Typst.Net.Configuration;

/// <summary>
/// Represents the configuration options for Typst.
/// </summary>
[ExcludeFromCodeCoverage]
public class TypstOptions
{
    private const int DefaultStreamBufferSize = 81920;

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

    /// <summary>
    /// Gets or sets the buffer size for streaming output from the Typst process.
    /// The default value is 81920 bytes, which is suitable for CopyToAsync operations to avoid small buffers.
    /// </summary>
    public int StdinBufferSize { get; set; } = DefaultStreamBufferSize;

    /// <summary>
    /// Gets or sets the buffer size for reading standard output from the Typst process.
    /// The default value is 81920 bytes, which is suitable for CopyToAsync operations to avoid small buffers.
    /// </summary>
    public int StdoutBufferSize { get; set; } = DefaultStreamBufferSize;

    /// <summary>
    /// Gets or sets the timeout for Typst process execution in milliseconds.
    /// The default value is 30000 milliseconds (30 seconds).
    /// </summary>
    public int DefaultTimeout { get; set; } = 30000;
}