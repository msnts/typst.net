using System.Diagnostics.CodeAnalysis;

namespace Typst.Net.Core;

[ExcludeFromCodeCoverage]
public sealed class TypstResult
{
    public bool IsSuccess { get; init; }
    public MemoryStream? Output { get; init; }
    public Error Error { get; init; }
    public string Details { get; init; }

    private TypstResult(MemoryStream output, string details)
    {
        Output = output;
        Error = Error.None;
        Details = details;
        IsSuccess = true;
    }
    private TypstResult(Error error, string details)
    {
        Output = null;
        Error = error;
        Details = details;
        IsSuccess = false;
    }

    public static TypstResult Success(MemoryStream output, string details) => new(output, details);
    public static TypstResult Failure(Error error, string details) => new(error, details);
}
