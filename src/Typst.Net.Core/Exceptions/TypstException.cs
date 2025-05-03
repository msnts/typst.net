namespace Typst.Net.Core.Exceptions;

public abstract class TypstException : Exception
{
    protected TypstException(string message) : base(message) { }
    protected TypstException(string message, Exception innerException) : base(message, innerException) { }
}