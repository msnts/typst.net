namespace Typst.Net.Core;

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
    /// Gets or sets a read-only dictionary of input files and their content.
    /// The keys represent file names, and the values represent their content.
    /// </summary>
    public IReadOnlyDictionary<string, string>? Inputs { get; set; }
}