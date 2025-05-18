using Microsoft.Extensions.Logging;
using System.Text;
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
        if (inputStream is null) return TypstResult.Failure("Input stream cannot be null.", string.Empty);
        if (compileOptions is null) return TypstResult.Failure("Compile options cannot be null.", string.Empty);
        if (!inputStream.CanRead) return TypstResult.Failure("Input stream must be readable.", string.Empty);

        TypstCompilerLogs.LogStartingCompilation(_logger, compileOptions.Format.ToString());

        try
        {
            var result = StartTypstProcess(compileOptions);

            if (result.IsError)
            {
                return TypstResult.Failure(result.Error, string.Empty);
            }

            using var process = result.Value;

            var compilationResult = await DoCompileAsync(process, inputStream, cancellationToken);

            TypstCompilerLogs.LogStdinTaskComplete(_logger, process.Id);

            string details = await process.GetStandardErrorAsStringAsync(cancellationToken);

            if (compilationResult.IsError)
            {
                _logger.LogError("Typst compilation failed (PID: {Id}) with exit code {exitCode}. Stderr:\n{Stderr}", process.Id, process.ExitCode, details);

                return TypstResult.Failure($"Typst process exited with code {process.ExitCode}.", details);
            }

            TypstCompilerLogs.LogProcessCompletedSuccessfully(_logger, process.Id);

            var outputResult = await ReadStdoutToMemoryAsync(process, cancellationToken);

            return outputResult.IsSuccess ? TypstResult.Success(outputResult.Value, details) : TypstResult.Failure(outputResult.Error, details);
        }
        catch (OperationCanceledException)
        {
            TypstCompilerLogs.LogCompilationCanceled(_logger, -1);
            throw;
        }
        catch (Exception ex)
        {
            TypstCompilerLogs.LogUnexpectedError(_logger, ex);
            return TypstResult.Failure($"Unexpected error during Typst compilation: {ex.Message}", ex.ToString());
        }
    }

    private async Task<Result<Unit>> DoCompileAsync(ITypstProcess process, Stream inputStream, CancellationToken cancellationToken)
    {
        try
        {
            // Concurrently write to stdin and wait for process exit
            Task<Result<Unit>> compilationTask = WriteToStdinAsync(process, inputStream, cancellationToken);

            TypstCompilerLogs.LogWaitingForProcessExit(_logger, process.Id);

            await process.WaitForExitAsync(cancellationToken);

            TypstCompilerLogs.LogProcessExited(_logger, process.Id);

            // Ensure the stdin writing task also completed
            return await compilationTask;
        }
        finally
        {
            CleanupProcess(process, "Unexpected error during Typst compilation.");
        }
    }

    private Result<ITypstProcess> StartTypstProcess(TypstCompileOptions compileOptions)
    {
        var process = _processFactory.CreateProcess(compileOptions);

        TypstCompilerLogs.LogStartingProcess(_logger);
        try
        {
            if (!process.Start())
            {
                return Result<ITypstProcess>.Failure("Failed to start Typst process (process.Start() returned false).");
            }
        }
        catch (Exception ex)
        {
            process.Dispose();
            TypstCompilerLogs.LogProcessStartFailed(_logger, ex);
            return Result<ITypstProcess>.Failure($"Failed to start Typst process: {ex.Message}");
        }

        TypstCompilerLogs.LogProcessStarted(_logger, process.Id);

        return Result<ITypstProcess>.Success(process);
    }

    private async Task<Result<Unit>> WriteToStdinAsync(ITypstProcess process, Stream inputStream, CancellationToken cancellationToken)
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
            return Result<Unit>.Success(Unit.Value);
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
            {
                return Result<Unit>.Failure($"IOException during stdin write for PID {process.Id}: {ioEx.Message}");
            }
            return Result<Unit>.Failure($"Typst process (PID: {process.Id}) exited with code {process.ExitCode} during stdin write: {ioEx.Message}");
        }
        catch (Exception ex)
        {
            TypstCompilerLogs.LogUnexpectedStdinError(_logger, ex, process.Id);
            return Result<Unit>.Failure($"Unexpected error during stdin write for PID {process.Id}: {ex.Message}");
        }
    }

    private async Task<Result<MemoryStream>> ReadStdoutToMemoryAsync(ITypstProcess process, CancellationToken cancellationToken)
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
            return Result<MemoryStream>.Success(memoryStream);
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
            return Result<MemoryStream>.Failure($"Failed to read stdout stream for PID {process.Id}: {ex.Message}");
        }
    }

    private void CleanupProcess(ITypstProcess? process, string reason)
    {
        if (process?.HasExited != false)
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