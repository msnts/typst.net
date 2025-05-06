using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Typst.Net.Core.Configuration;

namespace Typst.Net.Core.Process;

/// <summary>
/// Factory for creating Typst process instances.
/// </summary>
public class TypstProcessFactory(IOptions<TypstOptions> options, ILogger<TypstProcessFactory> logger) : ITypstProcessFactory
{
    private readonly TypstOptions _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    private readonly ILogger<TypstProcessFactory> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <inheritdoc />
    public ITypstProcess CreateProcess(TypstCompileOptions compileOptions)
    {
        ArgumentNullException.ThrowIfNull(compileOptions);
        string arguments = BuildArgumentsForStdinStdout(compileOptions);

        TypstCompilerLogs.LogCreatingProcess(_logger, _options.ExecutablePath, arguments);
        var processStartInfo = new ProcessStartInfo
        {
            FileName = _options.ExecutablePath,
            Arguments = arguments,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = compileOptions.RootDirectory ?? Environment.CurrentDirectory,
            StandardOutputEncoding = null,
            StandardErrorEncoding = Encoding.UTF8
        };

        return new TypstProcess(processStartInfo);
    }

    private string BuildArgumentsForStdinStdout(TypstCompileOptions options)
    {
        var argsBuilder = new StringBuilder();
        argsBuilder.Append("compile"); // Base command

        // Format is required
        argsBuilder.Append($" --format {options.Format.ToString().ToLowerInvariant()}");

        // Optional arguments
        if (options.FontPaths != null) {
            foreach(var path in options.FontPaths.Where(p => !string.IsNullOrWhiteSpace(p)))
                argsBuilder.Append($" --font-path \"{path.Trim()}\"");
        }
        if (options.Inputs != null) {
            foreach(var kvp in options.Inputs)
                argsBuilder.Append($" --input \"{kvp.Key.Trim()}\"=\"{kvp.Value}\""); // Basic escaping
        }
        if (!string.IsNullOrWhiteSpace(options.RootDirectory)) {
             argsBuilder.Append($" --root \"{options.RootDirectory.Trim()}\"");
        }

        // Specify stdin and stdout
        argsBuilder.Append(" -");   // Input from stdin
        argsBuilder.Append(" -"); // Output to stdout

        // Append default arguments if configured
        if (!string.IsNullOrWhiteSpace(_options.DefaultArguments)) {
            argsBuilder.Append($" {_options.DefaultArguments}");
        }

        return argsBuilder.ToString();
    }
} 