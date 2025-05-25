using System.Diagnostics;

namespace Typst.Net.Process;

/// <summary>
/// Interface representing a Typst process.
/// </summary>
public interface ITypstProcess : IDisposable
{
    /// <summary>
    /// Gets the process ID.
    /// </summary>
    int Id { get; }

    /// <summary>
    /// Gets the process start information.
    /// </summary>
    public ProcessStartInfo StartInfo { get; }

    /// <summary>
    /// Gets whether the process has exited.
    /// </summary>
    bool HasExited { get; }

    /// <summary>
    /// Gets the process exit code.
    /// </summary>
    int ExitCode { get; }

    /// <summary>
    /// Gets the standard input stream.
    /// </summary>
    Stream StandardInput { get; }

    /// <summary>
    /// Gets the standard output stream.
    /// </summary>
    Stream StandardOutput { get; }

    /// <summary>
    /// Gets the standard error stream.
    /// </summary>
    Stream StandardError { get; }

    /// <summary>
    /// Gets the standard output as a string.
    /// </summary>
    Task<string> GetStandardErrorAsStringAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts the process.
    /// </summary>
    /// <returns>True if the process was started successfully.</returns>
    bool Start();

    /// <summary>
    /// Waits for the process to exit.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task WaitForExitAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Kills the process.
    /// </summary>
    void Kill();
}