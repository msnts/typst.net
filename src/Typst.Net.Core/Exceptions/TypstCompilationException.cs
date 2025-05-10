using System.Diagnostics.CodeAnalysis;

namespace Typst.Net.Core.Exceptions;

[ExcludeFromCodeCoverage]
public class TypstCompilationException : TypstException
{
    public string StandardError { get; }

    public TypstCompilationException(string message, string stdErr) : base($"{message} See StandardError for details.")
    {
        StandardError = stdErr;
    }
}