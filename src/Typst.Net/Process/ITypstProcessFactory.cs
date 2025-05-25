namespace Typst.Net.Process;

/// <summary>
/// Factory interface for creating Typst process instances.
/// </summary>
public interface ITypstProcessFactory
{
    /// <summary>
    /// Creates a new instance of a Typst process.
    /// </summary>
    /// <param name="options">The compilation options.</param>
    /// <returns>A new instance of <see cref="ITypstProcess"/>.</returns>
    ITypstProcess CreateProcess(TypstCompileOptions options);
}