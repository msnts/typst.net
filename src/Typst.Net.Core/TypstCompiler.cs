using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;
using Typst.Net.Core.Configuration;
using Typst.Net.Core.Exceptions;

namespace Typst.Net.Core;

public class TypstCompiler : ITypstCompiler
{
    private readonly TypstOptions _options;
    private readonly ILogger<TypstCompiler> _logger;
    private const int DefaultStreamBufferSize = 81920; // Default for CopyToAsync, avoids small buffers

    public TypstCompiler(IOptions<TypstOptions> options, ILogger<TypstCompiler> logger)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<TypstResult> CompileAsync(Stream inputStream, TypstCompileOptions compileOptions, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(inputStream);
        ArgumentNullException.ThrowIfNull(compileOptions);
        if (!inputStream.CanRead) throw new ArgumentException("Input stream must be readable.", nameof(inputStream));

        _logger.LogInformation("Starting Typst compilation. Format={Format}", compileOptions.Format);

        Process? process = null;
        var errorBuilder = new StringBuilder();

        try
        {
            process = StartTypstProcess(compileOptions, errorBuilder);

            // Concurrently write to stdin and wait for process exit
            Task writingTask = WriteToStdinAsync(process, inputStream, cancellationToken);

            _logger.LogDebug("Asynchronously waiting for Typst process (PID: {PID}) to exit...", process.Id);

            await process.WaitForExitAsync(cancellationToken);

            _logger.LogDebug("Typst process exited (PID: {PID}). Proceeding with exit code check.", process.Id);

            // Ensure the stdin writing task also completed (it might finish after exit in some cases)
            // Although typically WaitForExitAsync won't return until IO pipes are flushed,
            // awaiting the writing task ensures any potential exceptions from it are observed.
            await writingTask;
            _logger.LogDebug("Stdin writing task confirmed complete (PID: {PID}).", process.Id);

            // Process has exited, check results and read stdout
            int exitCode = process.ExitCode;
            string stdErr = GetCapturedStderr(errorBuilder); // Get stderr *after* process exit

            if (exitCode == 0)
            {
                _logger.LogInformation("Typst process (PID: {PID}) completed successfully. Reading output stream.", process.Id);
                MemoryStream outputMemoryStream = await ReadStdoutToMemoryAsync(process, cancellationToken);
                // Process is disposed within this method scope (finally block)
                return new TypstResult(outputMemoryStream, stdErr);
            }
            else
            {
                // Compilation failed, create specific exception
                throw CreateCompilationException(exitCode, stdErr, process.Id);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Typst compilation was canceled (PID: {PID}).", process?.Id ?? -1);
            CleanupProcess(process, "cancellation");
            throw;
        }
        catch (Exception ex) when (ex is not TypstException)
        {
            _logger.LogError(ex, "Unexpected error during Typst compilation.");
            CleanupProcess(process, "unexpected error");
            throw new TypstProcessException($"Unexpected error during Typst execution: {ex.Message}", ex);
        }
        finally
        {
            if (process != null) 
            {
                if (process.ExitCode != 0)
                {
                    _logger.LogDebug("Typst process (PID: {PID}) resources released after error.", process.Id);
                }
                process.Dispose();
            }
        }
    }

    private Process StartTypstProcess(TypstCompileOptions compileOptions, StringBuilder errorBuilder)
    {
        string arguments = BuildArgumentsForStdinStdout(compileOptions);
        _logger.LogDebug("Creating Typst process. Executable='{Executable}', Args='{Arguments}'", _options.ExecutablePath, arguments);

        var processStartInfo = new ProcessStartInfo
        {
            FileName = _options.ExecutablePath,
            Arguments = arguments,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = compileOptions.RootDirectory ?? Environment.CurrentDirectory,
            StandardOutputEncoding = null, // Critical for binary stdout
            StandardErrorEncoding = Encoding.UTF8 // Standard practice for stderr
        };

        var process = new Process { StartInfo = processStartInfo, EnableRaisingEvents = true };

        process.ErrorDataReceived += (sender, args) => {
            if (args.Data != null) {
                lock (errorBuilder) { errorBuilder.AppendLine(args.Data); } // Thread-safe append
            }
        };

        _logger.LogDebug("Starting Typst process...");
        try
        {
             if (!process.Start())
             {
                process.Dispose();
                throw new TypstProcessException("Failed to start Typst process (process.Start() returned false).");
             }
        }
        catch (Exception ex)
        {
             process.Dispose();
             throw new TypstProcessException($"Failed to start Typst process: {ex.Message}", ex);
        }

        _logger.LogDebug("Typst process started successfully (PID: {PID}). Beginning stderr capture.", process.Id);
        process.BeginErrorReadLine();

        return process;
    }

    private async Task WriteToStdinAsync(Process process, Stream inputStream, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Starting copy from input stream to Typst stdin (PID: {PID}).", process.Id);
        try
        {
            // Use using to ensure StreamWriter disposal, which closes stdin pipe.
            // Specify UTF8 encoding, default buffer, and crucially leaveOpen: false.
            using (var stdinWriter = new StreamWriter(process.StandardInput.BaseStream, Encoding.UTF8, -1, leaveOpen: false))
            {
                await inputStream.CopyToAsync(stdinWriter.BaseStream, DefaultStreamBufferSize, cancellationToken);
                await stdinWriter.FlushAsync(cancellationToken); // Ensure buffer is flushed before closing
            }
            _logger.LogDebug("Finished writing to Typst stdin (PID: {PID}). Stdin stream closed.", process.Id);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Writing to Typst stdin (PID: {PID}) was canceled.", process.Id);
            throw;
        }
        catch (IOException ioEx)
        {
            _logger.LogError(ioEx, "IOException during stdin write (PID: {PID}). Process may have exited.", process.Id);
            if (process.HasExited)
                throw new TypstProcessException($"Typst process (PID: {process.Id}) exited with code {process.ExitCode} during stdin write.", ioEx);
            else
                throw new TypstProcessException($"IOException during stdin write for PID {process.Id}.", ioEx);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error writing to Typst stdin (PID: {PID}).", process.Id);
            throw new TypstProcessException($"Unexpected error during stdin write for PID {process.Id}.", ex);
        }
    }

    private async Task<MemoryStream> ReadStdoutToMemoryAsync(Process process, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Reading Typst stdout stream into memory (PID: {PID}).", process.Id);
        var memoryStream = new MemoryStream();
        try
        {
            // Access the raw byte stream directly
            using (var stdoutStream = process.StandardOutput.BaseStream)
            {
                await stdoutStream.CopyToAsync(memoryStream, DefaultStreamBufferSize, cancellationToken);
            }
            memoryStream.Position = 0; // Rewind stream for consumer
            _logger.LogDebug("Successfully read {BytesRead} bytes from stdout into memory (PID: {PID}).", memoryStream.Length, process.Id);
            return memoryStream;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Reading Typst stdout (PID: {PID}) was canceled.", process.Id);
            await memoryStream.DisposeAsync(); // Clean up partial stream
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading Typst stdout stream (PID: {PID}).", process.Id);
            await memoryStream.DisposeAsync();
            throw new TypstProcessException($"Failed to read stdout stream from PID {process.Id}.", ex);
        }
    }

    private static string GetCapturedStderr(StringBuilder errorBuilder)
    {
        lock (errorBuilder) // Ensure thread safety reading the shared builder
        {
            return errorBuilder.ToString();
        }
    }

    private TypstCompilationException CreateCompilationException(int exitCode, string stdErr, int processId)
    {
        string errorMsg = $"Typst compilation failed (PID: {processId}) with exit code {exitCode}.";
        _logger.LogError(errorMsg + " Stderr:\n{Stderr}", stdErr); // Log before throwing
        return new TypstCompilationException(errorMsg, stdErr);
    }

    private void CleanupProcess(Process? process, string reason)
    {
        if (process == null || process.HasExited)
        {
            return;
        }

        _logger.LogWarning("Attempting to kill Typst process (PID: {PID}) due to {Reason}.", process.Id, reason);
        
        try
        {
            process.Kill(true);
        }
        catch (Exception killEx)
        {
            _logger.LogWarning(killEx, "Failed to kill orphaned process (PID: {PID}). It might have exited already.", process.Id);
        }
    }

    private string BuildArgumentsForStdinStdout(TypstCompileOptions options)
    {
        var argsBuilder = new StringBuilder();
        argsBuilder.Append("compile"); // Base command

        // Format is required
        argsBuilder.Append($" --format {options.Format.ToString().ToLowerInvariant()}");

        // Optional arguments
        if (options.FontPaths != null) {
            foreach(var path in options.FontPaths.Where(p => !string.IsNullOrWhiteSpace(p)))
                argsBuilder.Append($" --font-path \"{path.Trim()}\"");
        }
        if (options.Inputs != null) {
            foreach(var kvp in options.Inputs)
                argsBuilder.Append($" --input \"{kvp.Key.Trim()}\"=\"{kvp.Value}\""); // Basic escaping
        }
        if (!string.IsNullOrWhiteSpace(options.RootDirectory)) {
             argsBuilder.Append($" --root \"{options.RootDirectory.Trim()}\"");
        }

        // Specify stdin and stdout
        argsBuilder.Append(" -");   // Input from stdin
        argsBuilder.Append(" -"); // Output to stdout

        // Append default arguments if configured
        if (!string.IsNullOrWhiteSpace(_options.DefaultArguments)) {
            argsBuilder.Append($" {_options.DefaultArguments}");
        }

        return argsBuilder.ToString();
    }
}