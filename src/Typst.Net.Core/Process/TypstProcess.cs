using System.Diagnostics;

namespace Typst.Net.Core.Process;

/// <summary>
/// Represents a Typst process instance.
/// </summary>
public class TypstProcess : ITypstProcess
{
    private readonly System.Diagnostics.Process _process;

    /// <summary>
    /// Initializes a new instance of the TypstProcess class.
    /// </summary>
    /// <param name="startInfo">The process start information.</param>
    public TypstProcess(ProcessStartInfo startInfo)
    {
        _process = new System.Diagnostics.Process { StartInfo = startInfo };
    }

    /// <inheritdoc />
    public bool HasExited => _process.HasExited;

    /// <inheritdoc />
    public int ExitCode => _process.ExitCode;

    /// <inheritdoc />
    public int Id => _process.Id;

    /// <inheritdoc />
    public Stream StandardInput => _process.StandardInput.BaseStream;

    /// <inheritdoc />
    public Stream StandardOutput => _process.StandardOutput.BaseStream;

    /// <inheritdoc />
    public Stream StandardError => _process.StandardError.BaseStream;

    /// <inheritdoc />
    public bool Start() => _process.Start();

    /// <inheritdoc />
    public Task WaitForExitAsync(CancellationToken cancellationToken = default) => 
        _process.WaitForExitAsync(cancellationToken);

    /// <inheritdoc />
    public void Kill() => _process.Kill();

    /// <inheritdoc />
    public void Dispose() => _process.Dispose();
} 