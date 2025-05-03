using Microsoft.Extensions.Options;

namespace Typst.Net.Core.Configuration;

public class TypstOptionsValidation : IValidateOptions<TypstOptions>
{
    public ValidateOptionsResult Validate(string? name, TypstOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.ExecutablePath))
        {
            return ValidateOptionsResult.Fail($"Typst executable path is not configured ('{TypstOptions.SectionName}.ExecutablePath').");
        }

        if (File.Exists(options.ExecutablePath))
        {
            return ValidateOptionsResult.Fail($"Typst executable not found at configured path: {options.ExecutablePath}");
        }

        return ValidateOptionsResult.Success;
    }
}