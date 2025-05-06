using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Typst.Net.Core.Configuration;
using Typst.Net.Core.Process;

namespace Typst.Net.Core.Tests;

public sealed class ProcessWrapperTests : IDisposable
{
    private readonly Mock<IOptions<TypstOptions>> _optionsMock;
    private readonly Mock<ILogger<TypstProcessFactory>> _loggerMock;
    private readonly TypstProcessFactory _factory;
    private readonly List<IDisposable> _disposables = new();

    public ProcessWrapperTests()
    {
        _optionsMock = new Mock<IOptions<TypstOptions>>();
        _loggerMock = new Mock<ILogger<TypstProcessFactory>>();

        _optionsMock.Setup(x => x.Value)
            .Returns(new TypstOptions { ExecutablePath = "typst" });

        _factory = new TypstProcessFactory(_optionsMock.Object, _loggerMock.Object);
    }

    public void Dispose()
    {
        foreach (var disposable in _disposables)
        {
            disposable.Dispose();
        }
    }

    [Fact]
    public void CreateProcess_WithNullOptions_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => _factory.CreateProcess(null!);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("compileOptions");
    }

    [Fact]
    public void CreateProcess_WithValidOptions_ReturnsProcess()
    {
        // Arrange
        var compileOptions = new TypstCompileOptions 
        { 
            Format = OutputFormat.Pdf,
            RootDirectory = "/tmp"
        };

        // Act
        var process = _factory.CreateProcess(compileOptions);
        _disposables.Add(process);

        // Assert
        process.Should().NotBeNull();
        process.Should().BeAssignableTo<ITypstProcess>();
    }
}