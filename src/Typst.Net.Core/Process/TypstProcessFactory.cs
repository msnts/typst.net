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
        string arguments = BuildArguments(compileOptions);

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

    private static string BuildArguments(TypstCompileOptions options)
    {
        var argsBuilder = new StringBuilder($"compile --format {options.Format.ToString().ToLowerInvariant()}");

        // Optional arguments
        if (options.FontPaths != null)
        {
            argsBuilder.AppendJoin(" ", options.FontPaths
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(path => $"--font-path \"{path.Trim()}\""));
        }

        if (string.IsNullOrWhiteSpace(options.Data))
        {
            argsBuilder.Append($" --input data={options.Data.Trim()}");
        }

        if (!string.IsNullOrWhiteSpace(options.RootDirectory)) {
             argsBuilder.Append($" --root \"{options.RootDirectory.Trim()}\"");
        }

        // Specify stdin and stdout
        argsBuilder.Append(" - -");

        return argsBuilder.ToString();
    }
} 