using System.Diagnostics.CodeAnalysis;

namespace Typst.Net.Core.Exceptions;

[ExcludeFromCodeCoverage]
public abstract class TypstException : Exception
{
    protected TypstException(string message) : base(message) { }
    protected TypstException(string message, Exception innerException) : base(message, innerException) { }
}