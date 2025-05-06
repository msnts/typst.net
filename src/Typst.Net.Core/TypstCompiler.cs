using Microsoft.Extensions.Logging;
using System.Text;
using Typst.Net.Core.Exceptions;
using Typst.Net.Core.Process;

namespace Typst.Net.Core;

public class TypstCompiler : ITypstCompiler
{
    private readonly ILogger<TypstCompiler> _logger;
    private const int DefaultStreamBufferSize = 81920; // Default for CopyToAsync, avoids small buffers

    private readonly ITypstProcessFactory _processFactory;

    public TypstCompiler(ILogger<TypstCompiler> logger, ITypstProcessFactory processFactory)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _processFactory = processFactory ?? throw new ArgumentNullException(nameof(processFactory));
    }

    public async Task<TypstResult> CompileAsync(Stream inputStream, TypstCompileOptions compileOptions, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(inputStream);
        ArgumentNullException.ThrowIfNull(compileOptions);
        if (!inputStream.CanRead) throw new ArgumentException("Input stream must be readable.", nameof(inputStream));

        TypstCompilerLogs.LogStartingCompilation(_logger, compileOptions.Format.ToString());

        ITypstProcess? process = null;

        try
        {
            process = StartTypstProcess(compileOptions);

            // Concurrently write to stdin and wait for process exit
            Task writingTask = WriteToStdinAsync(process, inputStream, cancellationToken);

            TypstCompilerLogs.LogWaitingForProcessExit(_logger, process.Id);

            await process.WaitForExitAsync(cancellationToken);

            TypstCompilerLogs.LogProcessExited(_logger, process.Id);

            // Ensure the stdin writing task also completed
            await writingTask;
            TypstCompilerLogs.LogStdinTaskComplete(_logger, process.Id);

            int exitCode = process.ExitCode;
            string stdErr = await new StreamReader(process.StandardError).ReadToEndAsync(cancellationToken);

            if (exitCode == 0)
            {
                TypstCompilerLogs.LogProcessCompletedSuccessfully(_logger, process.Id);
                var outputMemoryStream = await ReadStdoutToMemoryAsync(process, cancellationToken);
  
                return new TypstResult(outputMemoryStream, stdErr);
            }
            
            throw CreateCompilationException(exitCode, stdErr, process.Id);
        }
        catch (OperationCanceledException)
        {
            TypstCompilerLogs.LogCompilationCanceled(_logger, process?.Id ?? -1);
            CleanupProcess(process, "cancellation");
            throw;
        }
        catch (Exception ex) when (ex is not TypstException)
        {
            TypstCompilerLogs.LogUnexpectedError(_logger, ex);
            CleanupProcess(process, "unexpected error");
            throw new TypstProcessException($"Unexpected error during Typst execution: {ex.Message}", ex);
        }
        finally
        {
            process?.Dispose();
        }
    }

    private ITypstProcess StartTypstProcess(TypstCompileOptions compileOptions)
    {
        var process = _processFactory.CreateProcess(compileOptions);

        TypstCompilerLogs.LogStartingProcess(_logger);
        try
        {
            if (!process.Start())
            {
                throw new TypstProcessException("Failed to start Typst process (process.Start() returned false).");
            }
        }
        catch (Exception ex)
        {
            process.Dispose();
            TypstCompilerLogs.LogProcessStartFailed(_logger, ex);
            throw new TypstProcessException($"Failed to start Typst process: {ex.Message}", ex);
        }

        TypstCompilerLogs.LogProcessStarted(_logger, process.Id);

        return process;
    }

    private async Task WriteToStdinAsync(ITypstProcess process, Stream inputStream, CancellationToken cancellationToken)
    {
        TypstCompilerLogs.LogStartingStdinCopy(_logger, process.Id);
        try
        {
            using (var stdinWriter = new StreamWriter(process.StandardInput, Encoding.UTF8, -1, leaveOpen: false))
            {
                await inputStream.CopyToAsync(stdinWriter.BaseStream, DefaultStreamBufferSize, cancellationToken);
                await stdinWriter.FlushAsync(cancellationToken);
            }
            TypstCompilerLogs.LogFinishedStdinCopy(_logger, process.Id);
        }
        catch (OperationCanceledException)
        {
            TypstCompilerLogs.LogStdinWriteCanceled(_logger, process.Id);
            throw;
        }
        catch (IOException ioEx)
        {
            TypstCompilerLogs.LogStdinIOException(_logger, ioEx, process.Id);
            if (process.HasExited)
                throw new TypstProcessException($"Typst process (PID: {process.Id}) exited with code {process.ExitCode} during stdin write.", ioEx);
            else
                throw new TypstProcessException($"IOException during stdin write for PID {process.Id}.", ioEx);
        }
        catch (Exception ex)
        {
            TypstCompilerLogs.LogUnexpectedStdinError(_logger, ex, process.Id);
            throw new TypstProcessException($"Unexpected error during stdin write for PID {process.Id}.", ex);
        }
    }

    private async Task<MemoryStream> ReadStdoutToMemoryAsync(ITypstProcess process, CancellationToken cancellationToken)
    {
        TypstCompilerLogs.LogReadingStdout(_logger, process.Id);
        var memoryStream = new MemoryStream();
        try
        {
            using (var stdoutStream = process.StandardOutput)
            {
                await stdoutStream.CopyToAsync(memoryStream, DefaultStreamBufferSize, cancellationToken);
            }
            memoryStream.Position = 0;
            TypstCompilerLogs.LogStdoutReadComplete(_logger, memoryStream.Length, process.Id);
            return memoryStream;
        }
        catch (OperationCanceledException)
        {
            TypstCompilerLogs.LogStdoutReadCanceled(_logger, process.Id);
            await memoryStream.DisposeAsync();
            throw;
        }
        catch (Exception ex)
        {
            TypstCompilerLogs.LogStdoutReadError(_logger, ex, process.Id);
            await memoryStream.DisposeAsync();
            throw new TypstProcessException($"Failed to read stdout stream from PID {process.Id}.", ex);
        }
    }

    private TypstCompilationException CreateCompilationException(int exitCode, string stdErr, int processId)
    {
        string errorMsg = $"Typst compilation failed (PID: {processId}) with exit code {exitCode}.";
        _logger.LogError(errorMsg + " Stderr:\n{Stderr}", stdErr); // Log before throwing
        return new TypstCompilationException(errorMsg, stdErr);
    }

    private void CleanupProcess(ITypstProcess? process, string reason)
    {
        if (process == null || process.HasExited)
        {
            return;
        }

        TypstCompilerLogs.LogKillingProcess(_logger, process.Id, reason);
        
        try
        {
            process.Kill();
        }
        catch (Exception killEx)
        {
            TypstCompilerLogs.LogFailedToKillProcess(_logger, killEx, process.Id);
        }
    }
}