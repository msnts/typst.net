using System.Diagnostics;

namespace Typst.Net.Core.Process;

/// <summary>
/// Factory interface for creating Typst process instances.
/// </summary>
public interface ITypstProcessFactory
{
    /// <summary>
    /// Creates a new Typst process instance with the specified start information.
    /// </summary>
    /// <param name="startInfo">The process start information.</param>
    /// <returns>A new Typst process instance.</returns>
    ITypstProcess CreateProcess(ProcessStartInfo startInfo);
}