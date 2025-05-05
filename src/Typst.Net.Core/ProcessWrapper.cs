using System.Diagnostics;

namespace Typst.Net.Core;

/// <summary>
/// A concrete implementation of IProcessWrapper that creates ProcessInstance objects.
/// </summary>
public class ProcessWrapper : IProcessWrapper
{
    /// <inheritdoc />
    public IProcess CreateProcess(ProcessStartInfo startInfo)
    {
        ArgumentNullException.ThrowIfNull(startInfo, nameof(startInfo));
        return new ProcessInstance(startInfo);
    }
} 