using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using Typst.Net.Configuration;
using Typst.Net.Process;

namespace Typst.Net;

/// <inheritdoc />
public class TypstCompiler : ITypstCompiler
{
    private readonly TypstOptions _options;
    private readonly ILogger<TypstCompiler> _logger;
    private readonly ITypstProcessFactory _processFactory;

    public TypstCompiler(IOptions<TypstOptions> typstOptions, ITypstProcessFactory processFactory, ILogger<TypstCompiler> logger)
    {
        _options = typstOptions.Value ?? throw new ArgumentNullException(nameof(typstOptions));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _processFactory = processFactory ?? throw new ArgumentNullException(nameof(processFactory));
    }

    /// <inheritdoc />
    public async Task<TypstResult> CompileAsync(Stream inputStream, TypstCompileOptions compileOptions, CancellationToken cancellationToken = default)
    {
        if (inputStream is null) return TypstResult.ProcessError("Input stream cannot be null.");
        if (compileOptions is null) return TypstResult.ProcessError("Compile options cannot be null.");
        if (!inputStream.CanRead) return TypstResult.ProcessError("Input stream must be readable.");

        TypstCompilerLogs.LogStartingCompilation(_logger, compileOptions.Format.ToString());
        using var timeoutCts = CreateCancellationTokenSource(compileOptions, cancellationToken);
        var token = timeoutCts.Token;

        try
        {
            var result = StartTypstProcess(compileOptions);

            token.ThrowIfCancellationRequested();

            if (result.IsError)
            {
                return TypstResult.Failure(result.Error, string.Empty);
            }

            using var process = result.Value;

            var stderrTask = process.GetStandardErrorAsStringAsync(token);

            var compilationResult = await DoCompileAsync(process, inputStream, token);

            TypstCompilerLogs.LogStdinTaskComplete(_logger, process.Id);

            string details = await stderrTask;

            if (compilationResult.IsError)
            {
                TypstCompilerLogs.LogCompilationFailed(_logger, process.Id, process.ExitCode, details);

                return TypstResult.Failure(compilationResult.Error, details);
            }

            TypstCompilerLogs.LogProcessCompletedSuccessfully(_logger, process.Id);

            var outputResult = await ReadStdoutToMemoryAsync(process, token);

            return outputResult.IsSuccess
                ? TypstResult.Success(outputResult.Value, details)
                : TypstResult.Failure(outputResult.Error, details);
        }
        catch (OperationCanceledException)
        {
            TypstCompilerLogs.LogCompilationCanceled(_logger, -1);
            throw;
        }
        catch (Exception ex)
        {
            TypstCompilerLogs.LogUnexpectedError(_logger, ex);
            return TypstResult.ProcessError($"Unexpected error during Typst compilation: {ex.Message}");
        }
    }

    private CancellationTokenSource CreateCancellationTokenSource(TypstCompileOptions compileOptions, CancellationToken cancellationToken)
    {
        var timeout = TimeSpan.FromMilliseconds(compileOptions.Timeout > 0 ? compileOptions.Timeout : _options.DefaultTimeout);
        var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(timeout);
        return timeoutCts;
    }

    private async Task<Result<Unit>> DoCompileAsync(ITypstProcess process, Stream inputStream, CancellationToken cancellationToken)
    {
        try
        {
            var compilationResult = await WriteToStdinAsync(process, inputStream, cancellationToken);

            TypstCompilerLogs.LogWaitingForProcessExit(_logger, process.Id);

            await process.WaitForExitAsync(cancellationToken);

            TypstCompilerLogs.LogProcessExited(_logger, process.Id);

            if (compilationResult.IsError || (compilationResult.IsSuccess && process.ExitCode == 0))
            {
                return compilationResult;
            }

            return Result<Unit>.Failure(Error.CompilationError($"Typst process exited with code {process.ExitCode}."));
        }
        finally
        {
            CleanupProcess(process);
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
                return Result<ITypstProcess>.ProcessError("Failed to start Typst process (process.Start() returned false).");
            }
        }
        catch (Exception ex)
        {
            process.Dispose();
            TypstCompilerLogs.LogProcessStartFailed(_logger, ex);
            return Result<ITypstProcess>.ProcessError($"Failed to start Typst process: {ex.Message}");
        }

        TypstCompilerLogs.LogProcessStarted(_logger, process.Id);

        return Result<ITypstProcess>.Success(process);
    }

    private async Task<Result<Unit>> WriteToStdinAsync(ITypstProcess process, Stream inputStream, CancellationToken cancellationToken)
    {
        TypstCompilerLogs.LogStartingStdinCopy(_logger, process.Id);
        try
        {
            //await inputStream.CopyToAsync(process.StandardInput, DefaultStreamBufferSize, cancellationToken);
            await using (var stdinWriter = new StreamWriter(process.StandardInput, Encoding.UTF8, -1, leaveOpen: false))
            {
                await inputStream.CopyToAsync(stdinWriter.BaseStream, GetOptimalBufferSize(inputStream, _options.StdinBufferSize), cancellationToken);
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
                return Result<Unit>.ProcessError($"IOException during stdin write for PID {process.Id}: {ioEx.Message}");
            }
            return Result<Unit>.ProcessError($"Typst process (PID: {process.Id}) exited with code {process.ExitCode} during stdin write: {ioEx.Message}");
        }
        catch (Exception ex)
        {
            TypstCompilerLogs.LogUnexpectedStdinError(_logger, ex, process.Id);
            return Result<Unit>.ProcessError($"Unexpected error during stdin write for PID {process.Id}: {ex.Message}");
        }
    }

    private async Task<Result<MemoryStream>> ReadStdoutToMemoryAsync(ITypstProcess process, CancellationToken cancellationToken)
    {
        TypstCompilerLogs.LogReadingStdout(_logger, process.Id);
        var memoryStream = new MemoryStream();
        try
        {
            await process.StandardOutput.CopyToAsync(memoryStream, GetOptimalBufferSize(process.StandardOutput, _options.StdoutBufferSize), cancellationToken);

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
            return Result<MemoryStream>.ProcessError($"Failed to read stdout stream for PID {process.Id}: {ex.Message}");
        }
    }

    private void CleanupProcess(ITypstProcess? process)
    {
        if (process is not { HasExited: false })
        {
            return;
        }

        TypstCompilerLogs.LogKillingProcess(_logger, process.Id, "Unexpected error during Typst compilation.");

        try
        {
            process.Kill();
        }
        catch (Exception killEx)
        {
            TypstCompilerLogs.LogFailedToKillProcess(_logger, killEx, process.Id);
        }
    }

    private static int GetOptimalBufferSize(Stream stream, int defaultSize)
    {
        const int maxSensibleBufferSize = 4 * 1024 * 1024; // 4 MB

        if (!stream.CanSeek)
        {
            return Math.Min(defaultSize, maxSensibleBufferSize);
        }

        return stream.Length switch
        {
            > 0 and < 1024 * 1024 => (int)Math.Min(defaultSize, Math.Min(stream.Length, maxSensibleBufferSize)),
            _ => Math.Min(defaultSize, maxSensibleBufferSize)
        };
    }
}