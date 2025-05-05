using System.Diagnostics;
using FluentAssertions;
using Typst.Net.Core.Process;
using Xunit;

namespace Typst.Net.Core.Tests;

public sealed class ProcessWrapperTests : IDisposable
{
    private readonly TypstProcessFactory _factory;
    private readonly List<IDisposable> _disposables = new();

    public ProcessWrapperTests()
    {
        _factory = new TypstProcessFactory();
    }

    public void Dispose()
    {
        foreach (var disposable in _disposables)
        {
            disposable.Dispose();
        }
    }

    [Fact]
    public void CreateProcess_WithNullStartInfo_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => _factory.CreateProcess(null!);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("startInfo");
    }

    [Fact]
    public void CreateProcess_WithValidStartInfo_ReturnsProcess()
    {
        // Arrange
        var startInfo = new ProcessStartInfo
        {
            FileName = "typst",
            Arguments = "--version",
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        // Act
        var process = _factory.CreateProcess(startInfo);
        _disposables.Add(process);

        // Assert
        process.Should().NotBeNull();
        process.Should().BeAssignableTo<ITypstProcess>();
    }
}