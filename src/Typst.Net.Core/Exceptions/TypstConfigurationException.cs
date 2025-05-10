using System.Diagnostics.CodeAnalysis;

namespace Typst.Net.Core.Exceptions;

[ExcludeFromCodeCoverage]
public class TypstConfigurationException : TypstException
{
    public TypstConfigurationException(string message) : base(message) { }
}