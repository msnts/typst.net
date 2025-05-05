using System.Diagnostics;
using System.Text;

namespace Typst.Net.Core;

/// <summary>
/// A concrete implementation of IProcess that wraps System.Diagnostics.Process.
/// </summary>
public class ProcessInstance : IProcess
{
    private readonly Process _process;
    private readonly StringBuilder _errorBuilder = new();

    /// <summary>
    /// Initializes a new instance of the ProcessInstance class.
    /// </summary>
    /// <param name="startInfo">The process start information.</param>
    public ProcessInstance(ProcessStartInfo startInfo)
    {
        _process = new Process { StartInfo = startInfo };
        _process.ErrorDataReceived += OnErrorDataReceived;
    }

    private void OnErrorDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (e.Data != null)
        {
            lock (_errorBuilder)
            {
                _errorBuilder.AppendLine(e.Data);
            }
        }
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
    public string GetErrorData()
    {
        lock (_errorBuilder)
        {
            return _errorBuilder.ToString();
        }
    }

    /// <inheritdoc />
    public bool Start()
    {
        var started = _process.Start();
        if (started)
        {
            _process.BeginErrorReadLine();
        }
        return started;
    }

    /// <inheritdoc />
    public Task WaitForExitAsync(CancellationToken cancellationToken = default) => 
        _process.WaitForExitAsync(cancellationToken);

    /// <inheritdoc />
    public void Kill() => _process.Kill();

    /// <inheritdoc />
    public void Dispose()
    {
        _process.ErrorDataReceived -= OnErrorDataReceived;
        _process.Dispose();
        GC.SuppressFinalize(this);
    }
} 