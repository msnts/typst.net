namespace Typst.Net.Core;

/// <summary>
/// Represents a compiler interface for processing Typst documents.
/// </summary>
public interface ITypstCompiler
{
    /// <summary>
    /// Compiles a Typst document asynchronously from the provided input stream using the specified options.
    /// </summary>
    /// <param name="inputStream">The input stream containing the Typst document to compile.</param>
    /// <param name="compileOptions">The options to customize the compilation process.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous compilation operation. The task result contains the compilation result.</returns>
    Task<TypstResult> CompileAsync(Stream inputStream, TypstCompileOptions compileOptions, CancellationToken cancellationToken = default);
}