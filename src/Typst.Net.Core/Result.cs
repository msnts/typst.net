namespace Typst.Net.Core;

internal readonly struct Unit
{
    public static readonly Unit Value = new();
}

internal class Result<T>
{
    public bool IsSuccess { get; init; }
    public bool IsError => !IsSuccess;
    public Error Error { get; init; }
    public T Value { get; init; }

    public Result(T value)
    {
        IsSuccess = true;
        Error = Error.None;
        Value = value ?? throw new ArgumentNullException(nameof(value), "Value cannot be null.");
    }

    public Result(Error error)
    {
        IsSuccess = false;
        Error = error ?? throw new ArgumentNullException(nameof(error), "Error cannot be null.");
        Value = default!;
    }

    public static Result<T> Success(T value) => new(value);
    public static Result<T> Failure(Error error) => new(error);
    public static Result<T> ProcessError(string error) => new(Error.ProcessError(error));
}