namespace Typst.Net.Core.Exceptions;

public class TypstProcessException : TypstException
{
    public TypstProcessException(string message) : base(message) { }
    public TypstProcessException(string message, Exception innerException) : base(message, innerException) { }
}
