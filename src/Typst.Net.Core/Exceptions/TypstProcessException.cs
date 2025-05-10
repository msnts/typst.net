using System.Diagnostics.CodeAnalysis;

namespace Typst.Net.Core.Exceptions;

[ExcludeFromCodeCoverage]
public class TypstProcessException : TypstException
{
    public TypstProcessException(string message) : base(message) { }
    public TypstProcessException(string message, Exception innerException) : base(message, innerException) { }
}
