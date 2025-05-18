using System.Diagnostics.CodeAnalysis;

namespace Typst.Net.Core;

/// <summary>
/// Represents the result of a Typst operation, indicating either success with output or failure with an error.
/// </summary>
/// <remarks>
/// This class encapsulates the outcome of a Typst operation, providing information about success state,
/// output stream, error details, and additional information.
/// </remarks>
[ExcludeFromCodeCoverage]
public sealed class TypstResult
{
    /// <summary>
    /// Gets a value indicating whether the operation was successful.
    /// </summary>
    /// <remarks>
    /// If <c>true</c>, the operation was successful; otherwise, <c>false</c>.
    /// </remarks>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// Gets the output of the operation as a <see cref="MemoryStream"/> if successful; otherwise, <c>null</c>.
    /// </summary>
    /// <remarks>
    /// The output stream contains the result of the operation if it was successful; otherwise, <c>null</c>.
    /// </remarks>
    public MemoryStream? Output { get; init; }

    /// <summary>
    /// Gets the error details if the operation failed; otherwise, <see cref="Error.None"/>.
    /// </summary>
    /// <remarks>
    /// If the operation failed, this property contains the error details; otherwise, it contains <see cref="Error.None"/>.
    /// </remarks>
    /// <summary>
    /// <see cref="Error.None"/> if the operation was successful.
    /// </remarks>
    public Error Error { get; init; }

    /// <summary>
    /// Gets additional details about the operation.
    /// </summary>
    /// <remarks>
    /// This property may contain additional information about the operation, such as warnings or other relevant details.
    /// </remarks>
    /// <seealso cref="Error"/>
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

    internal static TypstResult Success(MemoryStream output, string details) => new(output, details);
    internal static TypstResult Failure(Error error, string details) => new(error, details);
}
