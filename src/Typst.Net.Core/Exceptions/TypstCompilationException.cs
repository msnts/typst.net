namespace Typst.Net.Core.Exceptions;

public class TypstCompilationException : TypstException
{
    public string StandardError { get; }
    // Note: StandardOutput is not included as we don't capture text stdout anymore
    public TypstCompilationException(string message, string stdErr) : base($"{message} See StandardError for details.")
    {
        StandardError = stdErr;
    }
}