namespace Typst.Net.Core.Process;

public readonly struct Unit
{
    public static readonly Unit Value = new();
}

internal class Result<T>
{
    public bool IsSuccess { get; init; }
    public bool IsError => !IsSuccess;
    public string Error { get; init; }
    public T Value { get; init; }

    public Result(T value)
    {
        IsSuccess = true;
        Error = string.Empty;
        Value = value ?? throw new ArgumentNullException(nameof(value), "Value cannot be null.");
    }

    public Result(string error)
    {
        IsSuccess = false;
        Error = error ?? throw new ArgumentNullException(nameof(error), "Error cannot be null.");
        Value = default!;
    }
    public static Result<T> Success(T value) => new(value);
    public static Result<T> Failure(string error) => new(error);
}