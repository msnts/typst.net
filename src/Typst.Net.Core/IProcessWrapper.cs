using System.Diagnostics;

namespace Typst.Net.Core;

/// <summary>
/// Represents a factory interface for creating process instances to enable unit testing.
/// </summary>
public interface IProcessWrapper
{
    /// <summary>
    /// Creates a new process instance with the specified start information.
    /// </summary>
    /// <param name="startInfo">The process start information.</param>
    /// <returns>A new process instance.</returns>
    IProcess CreateProcess(ProcessStartInfo startInfo);
}

/// <summary>
/// Represents a process instance interface to enable unit testing.
/// </summary>
public interface IProcess : IDisposable
{
    /// <summary>
    /// Gets a value indicating whether the process has exited.
    /// </summary>
    bool HasExited { get; }

    /// <summary>
    /// Gets the process exit code.
    /// </summary>
    int ExitCode { get; }

    /// <summary>
    /// Gets the process ID.
    /// </summary>
    int Id { get; }

    /// <summary>
    /// Gets a stream used to write the input of the application.
    /// </summary>
    Stream StandardInput { get; }

    /// <summary>
    /// Gets a stream used to read the output of the application.
    /// </summary>
    Stream StandardOutput { get; }

    /// <summary>
    /// Gets a stream used to read the error output of the application.
    /// </summary>
    Stream StandardError { get; }

    /// <summary>
    /// Gets the error data received from the process.
    /// </summary>
    string GetErrorData();

    /// <summary>
    /// Starts the process.
    /// </summary>
    /// <returns>true if a process resource is started; false if no new process resource is started.</returns>
    bool Start();

    /// <summary>
    /// Instructs the Process component to wait indefinitely for the associated process to exit.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task WaitForExitAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Immediately stops the associated process.
    /// </summary>
    void Kill();
}
