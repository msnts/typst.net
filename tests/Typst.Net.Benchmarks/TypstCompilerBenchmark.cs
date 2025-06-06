using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using Typst.Net;
using Typst.Net.Configuration;
using Typst.Net.Process;

namespace Typst.Net.Benchmarks;

#pragma warning disable CS8618, IDISP006 

[MemoryDiagnoser(true)]
public class TypstCompilerBenchmark
{
    private TypstCompiler _compiler;
    private MemoryStream _inputStream;
    private TypstCompileOptions _compileOptions;
    private TypstResult? _result;

    [GlobalSetup]
    public void Setup()
    {
        var options = Options.Create(new TypstOptions
        {
            ExecutablePath = "/home/marcos/.cargo/bin/typst"
        });
        using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

        var logger = loggerFactory.CreateLogger<TypstCompiler>();
        var processFactory = new TypstProcessFactory(options, loggerFactory.CreateLogger<TypstProcessFactory>());
        _compiler = new TypstCompiler(options, processFactory, logger);

        // Criar um stream de entrada simulado
        const string sampleInput = "Sample Typst document content";
        _inputStream?.Dispose();
        _inputStream = new MemoryStream(Encoding.UTF8.GetBytes(sampleInput));

        // Configurar opções de compilação simuladas
        _compileOptions = new TypstCompileOptions
        {
            Format = OutputFormat.Pdf
        };
    }

    [Benchmark]
    public async Task CompileAsyncBenchmark()
    {
        // Executar o método CompileAsync
        _result = await _compiler.CompileAsync(_inputStream, _compileOptions, CancellationToken.None);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _inputStream.Dispose();
        _result?.Output?.Dispose();
    }
}