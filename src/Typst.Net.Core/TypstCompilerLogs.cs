using Microsoft.Extensions.Logging;

public static partial class TypstCompilerLogs
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Starting Typst compilation. Format={Format}")]
    public static partial void LogStartingCompilation(ILogger logger, string format);

    [LoggerMessage(EventId = 2, Level = LogLevel.Debug, Message = "Asynchronously waiting for Typst process (PID: {PID}) to exit...")]
    public static partial void LogWaitingForProcessExit(ILogger logger, int pid);

    [LoggerMessage(EventId = 3, Level = LogLevel.Debug, Message = "Typst process exited (PID: {PID}). Proceeding with exit code check.")]
    public static partial void LogProcessExited(ILogger logger, int pid);

    [LoggerMessage(EventId = 4, Level = LogLevel.Debug, Message = "Stdin writing task confirmed complete (PID: {PID}).")]
    public static partial void LogStdinTaskComplete(ILogger logger, int pid);

    [LoggerMessage(EventId = 5, Level = LogLevel.Information, Message = "Typst process (PID: {PID}) completed successfully. Reading output stream.")]
    public static partial void LogProcessCompletedSuccessfully(ILogger logger, int pid);

    [LoggerMessage(EventId = 6, Level = LogLevel.Warning, Message = "Typst compilation was canceled (PID: {PID}).")]
    public static partial void LogCompilationCanceled(ILogger logger, int pid);

    [LoggerMessage(EventId = 7, Level = LogLevel.Error, Message = "Unexpected error during Typst compilation.")]
    public static partial void LogUnexpectedError(ILogger logger, Exception exception);

    [LoggerMessage(EventId = 8, Level = LogLevel.Warning, Message = "Attempting to kill Typst process (PID: {PID}) due to {Reason}.")]
    public static partial void LogKillingProcess(ILogger logger, int pid, string reason);

    [LoggerMessage(EventId = 9, Level = LogLevel.Debug, Message = "Typst process (PID: {PID}) resources released after error.")]
    public static partial void LogResourcesReleased(ILogger logger, int pid);

    [LoggerMessage(EventId = 10, Level = LogLevel.Debug, Message = "Starting copy from input stream to Typst stdin (PID: {PID}).")]
    public static partial void LogStartingStdinCopy(ILogger logger, int pid);

    [LoggerMessage(EventId = 11, Level = LogLevel.Debug, Message = "Finished writing to Typst stdin (PID: {PID}). Stdin stream closed.")]
    public static partial void LogFinishedStdinCopy(ILogger logger, int pid);

    [LoggerMessage(EventId = 12, Level = LogLevel.Warning, Message = "Writing to Typst stdin (PID: {PID}) was canceled.")]
    public static partial void LogStdinWriteCanceled(ILogger logger, int pid);

    [LoggerMessage(EventId = 13, Level = LogLevel.Error, Message = "IOException during stdin write (PID: {PID}). Process may have exited.")]
    public static partial void LogStdinIOException(ILogger logger, IOException exception, int pid);

    [LoggerMessage(EventId = 14, Level = LogLevel.Error, Message = "Unexpected error writing to Typst stdin (PID: {PID}).")]
    public static partial void LogUnexpectedStdinError(ILogger logger, Exception exception, int pid);

    [LoggerMessage(EventId = 15, Level = LogLevel.Debug, Message = "Reading Typst stdout stream into memory (PID: {PID}).")]
    public static partial void LogReadingStdout(ILogger logger, int pid);

    [LoggerMessage(EventId = 16, Level = LogLevel.Debug, Message = "Successfully read {BytesRead} bytes from stdout into memory (PID: {PID}).")]
    public static partial void LogStdoutReadComplete(ILogger logger, long bytesRead, int pid);

    [LoggerMessage(EventId = 17, Level = LogLevel.Warning, Message = "Reading Typst stdout (PID: {PID}) was canceled.")]
    public static partial void LogStdoutReadCanceled(ILogger logger, int pid);

    [LoggerMessage(EventId = 18, Level = LogLevel.Error, Message = "Error reading Typst stdout stream (PID: {PID}).")]
    public static partial void LogStdoutReadError(ILogger logger, Exception exception, int pid);

    [LoggerMessage(EventId = 19, Level = LogLevel.Warning, Message = "Failed to kill orphaned process (PID: {PID}). It might have exited already.")]
    public static partial void LogFailedToKillProcess(ILogger logger, Exception exception, int pid);

    [LoggerMessage(EventId = 20, Level = LogLevel.Debug, Message = "Creating Typst process. Executable='{Executable}', Args='{Arguments}'")]
    public static partial void LogCreatingProcess(ILogger logger, string executable, string arguments);

    [LoggerMessage(EventId = 21, Level = LogLevel.Debug, Message = "Starting Typst process...")]
    public static partial void LogStartingProcess(ILogger logger);

    [LoggerMessage(EventId = 22, Level = LogLevel.Debug, Message = "Typst process started successfully (PID: {PID}). Beginning stderr capture.")]
    public static partial void LogProcessStarted(ILogger logger, int pid);

    [LoggerMessage(EventId = 23, Level = LogLevel.Error, Message = "Failed to start Typst process.")]
    public static partial void LogProcessStartFailed(ILogger logger, Exception exception);
}