namespace Typst.Net;

public enum ErrorCode
{
    None = 0,
    CompilationError = 1,
    ProcessError = 2
}

public sealed record Error(ErrorCode Code, string Description)
{
    public static Error None => new(ErrorCode.None, "No error");

    public static Error CompilationError(string description) => new(ErrorCode.CompilationError, description);
    public static Error ProcessError(string description) => new(ErrorCode.ProcessError, description);
}