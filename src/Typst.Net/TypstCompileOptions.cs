using System.Diagnostics.CodeAnalysis;

namespace Typst.Net;

/// <summary>
/// Specifies the output format for the compilation process.
/// </summary>
public enum OutputFormat
{
    /// <summary>
    /// Output format is PDF.
    /// </summary>
    Pdf,

    /// <summary>
    /// Output format is SVG.
    /// </summary>
    Svg,

    /// <summary>
    /// Output format is PNG.
    /// </summary>
    Png
}

/// <summary>
/// Represents the options used during the Typst compilation process.
/// </summary>
[ExcludeFromCodeCoverage]
public class TypstCompileOptions
{
    /// <summary>
    /// Gets or sets the desired output format for the compilation.
    /// Default is <see cref="OutputFormat.Pdf"/>.
    /// </summary>
    public OutputFormat Format { get; set; } = OutputFormat.Pdf;

    /// <summary>
    /// Gets or sets the root directory for the compilation process.
    /// This can be used to resolve relative paths.
    /// </summary>
    public string? RootDirectory { get; set; }

    /// <summary>
    /// Gets or sets the collection of font file paths to be used during compilation.
    /// </summary>
    public IEnumerable<string>? FontPaths { get; set; }

    /// <summary>
    /// Gets or sets the data to be included in the compilation process.
    /// </summary>
    public string Data { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timeout duration for the compilation process in milliseconds.
    /// A value of -1 indicates no timeout.
    /// </summary>
    public int Timeout { get; set; } = -1;
}