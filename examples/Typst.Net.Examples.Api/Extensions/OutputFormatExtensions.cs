using Typst.Net;

namespace Typst.Net.Examples.Api.Extensions;

public static class OutputFormatExtensions
{
    /// <summary>
    /// Converts the specified <see cref="OutputFormat"/> to its corresponding MIME type.
    /// </summary>
    /// <param name="outputFormat">The output format to convert.</param>
    /// <returns>The MIME type as a string.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the output format is not recognized.</exception>
    public static string ToContentType(this OutputFormat outputFormat)
    {
        return outputFormat switch
        {
            OutputFormat.Pdf => "application/pdf",
            OutputFormat.Svg => "image/svg+xml",
            OutputFormat.Png => "image/png",
            _ => throw new ArgumentOutOfRangeException(nameof(outputFormat), outputFormat, null)
        };
    }
}