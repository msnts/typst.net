using System.Diagnostics;

namespace Typst.Net.Core.Process;

/// <summary>
/// Factory for creating Typst process instances.
/// </summary>
public class TypstProcessFactory : ITypstProcessFactory
{
    /// <inheritdoc />
    public ITypstProcess CreateProcess(ProcessStartInfo startInfo)
    {
        ArgumentNullException.ThrowIfNull(startInfo, nameof(startInfo));
        return new TypstProcess(startInfo);
    }
} 