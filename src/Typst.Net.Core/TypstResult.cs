using System.Diagnostics.CodeAnalysis;

namespace Typst.Net.Core;

[ExcludeFromCodeCoverage]
public sealed class TypstResult
{
    public bool IsSuccess { get; init; }
    public MemoryStream? Output { get; init; }
    public string Error { get; init; }
    public string Details { get; init; }

    private TypstResult(MemoryStream output, string details)
    {
        Output = output;
        Error = string.Empty;
        Details = details;
        IsSuccess = true;
    }
    private TypstResult(string error, string details)
    {
        Output = null;
        Error = error;
        Details = details;
        IsSuccess = false;
    }

    public static TypstResult Success(MemoryStream output, string details) => new(output, details);
    public static TypstResult Failure(string error, string details) => new(error, details);
}
