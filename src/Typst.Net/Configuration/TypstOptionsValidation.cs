using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Options;

namespace Typst.Net.Configuration;

[ExcludeFromCodeCoverage]
public class TypstOptionsValidation : IValidateOptions<TypstOptions>
{
    public ValidateOptionsResult Validate(string? name, TypstOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.ExecutablePath))
        {
            return ValidateOptionsResult.Fail(
                "Typst executable path is not configured. " +
                $"Please set the '{TypstOptions.ExecutablePathEnvVar}' environment variable " +
                $"or configure '{TypstOptions.SectionName}.ExecutablePath' in your configuration.");
        }

        if (!File.Exists(options.ExecutablePath))
        {
            return ValidateOptionsResult.Fail(
                $"Typst executable not found at configured path: {options.ExecutablePath}. " +
                "Please ensure the path is correct and the executable exists.");
        }

        return ValidateOptionsResult.Success;
    }
}