using System.Diagnostics;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Typst.Net.Core.Configuration;
using Typst.Net.Core.Process;

namespace Typst.Net.Core.Tests;

public sealed class TypstCompilerTests : IDisposable
{
    private readonly Mock<ITypstProcessFactory> _processFactoryMock;
    private readonly Mock<ILogger<TypstCompiler>> _loggerMock;
    private readonly TypstCompiler _compiler;
    private readonly List<IDisposable> _disposables = new();

    public TypstCompilerTests()
    {
        _processFactoryMock = new Mock<ITypstProcessFactory>();
        _loggerMock = new Mock<ILogger<TypstCompiler>>();

        _compiler = new TypstCompiler(
            _loggerMock.Object,
            _processFactoryMock.Object);
    }

    public void Dispose()
    {
        foreach (var disposable in _disposables)
        {
            disposable.Dispose();
        }
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange
        var processFactory = Mock.Of<ITypstProcessFactory>();

        // Act & Assert
        var act = () => new TypstCompiler(null!, processFactory);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    [Fact]
    public void Constructor_WithNullProcessFactory_ThrowsArgumentNullException()
    {
        // Arrange
        var logger = Mock.Of<ILogger<TypstCompiler>>();

        // Act & Assert
        var act = () => new TypstCompiler(logger, null!);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("processFactory");
    }

    [Fact]
    public async Task CompileAsync_WithNullInputStream_ReturnsFailureResult()
    {
        // Arrange
        var compileOptions = new TypstCompileOptions();

        // Act
        var result = await _compiler.CompileAsync(null!, compileOptions);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.ToString().Should().Contain("Input stream cannot be null");
    }

    [Fact]
    public async Task CompileAsync_WithNullCompileOptions_ReturnsFailureResult()
    {
        // Arrange
        var inputStream = new MemoryStream();
        _disposables.Add(inputStream);

        // Act
        var result = await _compiler.CompileAsync(inputStream, null!);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Description.Should().Be("Compile options cannot be null.");
    }

    [Fact]
    public async Task CompileAsync_WithNonReadableStream_ReturnsFailureResult()
    {
        // Arrange
        var inputStream = new MemoryStream();
        inputStream.Close();
        _disposables.Add(inputStream);

        var compileOptions = new TypstCompileOptions();

        // Act
        var result = await _compiler.CompileAsync(inputStream, compileOptions);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Description.Should().Be("Input stream must be readable.");
    }

    [Fact]
    public async Task CompileAsync_WithValidInput_ReturnsSuccessfulResult()
    {
        // Arrange
        var inputStream = new MemoryStream(Encoding.UTF8.GetBytes("Hello, Typst!"));
        _disposables.Add(inputStream);

        var compileOptions = new TypstCompileOptions { Format = OutputFormat.Pdf };
        var outputStream = new MemoryStream(Encoding.UTF8.GetBytes("PDF content"));
        _disposables.Add(outputStream);

        var errorStream = new MemoryStream(40);
        _disposables.Add(errorStream);

        var processInputStream = new MemoryStream();
        _disposables.Add(processInputStream);

        var processMock = new Mock<ITypstProcess>();
        processMock.Setup(x => x.Start()).Returns(true);
        processMock.Setup(x => x.HasExited).Returns(true);
        processMock.Setup(x => x.ExitCode).Returns(0);
        processMock.Setup(x => x.StandardInput).Returns(processInputStream);
        processMock.Setup(x => x.StandardOutput).Returns(outputStream);
        processMock.Setup(x => x.StandardError).Returns(errorStream);
        processMock.Setup(x => x.Id).Returns(12345);

        _processFactoryMock.Setup(x => x.CreateProcess(It.IsAny<TypstCompileOptions>()))
            .Returns(processMock.Object);

        // Act
        var result = await _compiler.CompileAsync(inputStream, compileOptions);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Output.Should().NotBeNull();
        result.Output!.Length.Should().BeGreaterThan(0);

        _processFactoryMock.Verify(x => x.CreateProcess(It.Is<TypstCompileOptions>(o => 
            o.Format == OutputFormat.Pdf)), Times.Once);

        processMock.Verify(x => x.Start(), Times.Once);
        processMock.Verify(x => x.WaitForExitAsync(It.IsAny<CancellationToken>()), Times.Once);
        processMock.Verify(x => x.Dispose(), Times.Once);
    }

    [Fact]
    public async Task CompileAsync_WhenProcessFails_ReturnsFailureResult()
    {
        // Arrange
        var inputStream = new MemoryStream(Encoding.UTF8.GetBytes("Hello, Typst!"));
        _disposables.Add(inputStream);

        var compileOptions = new TypstCompileOptions { Format = OutputFormat.Pdf };
        var errorStream = new MemoryStream(Encoding.UTF8.GetBytes("Compilation error"));
        _disposables.Add(errorStream);

        var processInputStream = new MemoryStream();
        _disposables.Add(processInputStream);

        var processMock = new Mock<ITypstProcess>();
        processMock.Setup(x => x.Start()).Returns(true);
        processMock.Setup(x => x.HasExited).Returns(true);
        processMock.Setup(x => x.ExitCode).Returns(1);
        processMock.Setup(x => x.StandardInput).Returns(processInputStream);
        processMock.Setup(x => x.StandardError).Returns(errorStream);
        processMock.Setup(x => x.Id).Returns(12345);

        _processFactoryMock.Setup(x => x.CreateProcess(It.IsAny<TypstCompileOptions>()))
            .Returns(processMock.Object);

        // Act
        var result = await _compiler.CompileAsync(inputStream, compileOptions);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be(ErrorCode.CompilationError);
        result.Error.Description.Should().Contain("Typst process exited with code 1");

        processMock.Verify(x => x.Dispose(), Times.Once);
    }

    [Fact]
    public async Task CompileAsync_WhenProcessStartFails_ReturnsFailureResult()
    {
        // Arrange
        var inputStream = new MemoryStream(Encoding.UTF8.GetBytes("Hello, Typst!"));
        _disposables.Add(inputStream);

        var compileOptions = new TypstCompileOptions { Format = OutputFormat.Pdf };

        var processMock = new Mock<ITypstProcess>();
        processMock.Setup(x => x.Start()).Returns(false);

        _processFactoryMock.Setup(x => x.CreateProcess(It.IsAny<TypstCompileOptions>()))
            .Returns(processMock.Object);

        // Act
        var result = await _compiler.CompileAsync(inputStream, compileOptions);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Description.Should().Contain("Failed to start Typst process");

        processMock.Verify(x => x.Dispose(), Times.Never);
    }

    [Fact]
    public async Task CompileAsync_WhenCancelled_ThrowsOperationCanceledException()
    {
        // Arrange
        var inputStream = new MemoryStream(Encoding.UTF8.GetBytes("Hello, Typst!"));
        _disposables.Add(inputStream);

        var compileOptions = new TypstCompileOptions { Format = OutputFormat.Pdf };
        var cancellationToken = new CancellationToken(true);

        var processMock = new Mock<ITypstProcess>();
        processMock.Setup(x => x.Start()).Returns(true);
        processMock.Setup(x => x.WaitForExitAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());
        processMock.Setup(x => x.Id).Returns(12345);

        _processFactoryMock.Setup(x => x.CreateProcess(It.IsAny<TypstCompileOptions>()))
            .Returns(processMock.Object);

        // Act & Assert
        var act = () => _compiler.CompileAsync(inputStream, compileOptions, cancellationToken);
        await act.Should().ThrowAsync<OperationCanceledException>();

        processMock.Verify(x => x.Kill(), Times.Once);
        processMock.Verify(x => x.Dispose(), Times.Once);
    }

    [Fact]
    public async Task CompileAsync_WhenStdinWriteFails_ReturnsFailureResult()
    {
        // Arrange
        var inputStream = new MemoryStream(Encoding.UTF8.GetBytes("Hello, Typst!"));
        _disposables.Add(inputStream);

        var compileOptions = new TypstCompileOptions { Format = OutputFormat.Pdf };

        var processMock = new Mock<ITypstProcess>();
        processMock.Setup(x => x.Start()).Returns(true);
        processMock.Setup(x => x.StandardInput).Throws(new IOException("Write failed"));
        processMock.Setup(x => x.Id).Returns(12345);

        _processFactoryMock.Setup(x => x.CreateProcess(It.IsAny<TypstCompileOptions>()))
            .Returns(processMock.Object);

        // Act
        var result = await _compiler.CompileAsync(inputStream, compileOptions);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Description.Should().Contain("Typst process (PID:");

        processMock.Verify(x => x.Dispose(), Times.Once);
    }

    [Fact]
    public async Task CompileAsync_WhenStdoutReadFails_ReturnsFailureResult()
    {
        // Arrange
        var inputStream = new MemoryStream(Encoding.UTF8.GetBytes("Hello, Typst!"));
        _disposables.Add(inputStream);

        var compileOptions = new TypstCompileOptions { Format = OutputFormat.Pdf };
        var errorStream = new MemoryStream();
        _disposables.Add(errorStream);

        var processInputStream = new MemoryStream();
        _disposables.Add(processInputStream);

        var processMock = new Mock<ITypstProcess>();
        processMock.Setup(x => x.Start()).Returns(true);
        processMock.Setup(x => x.HasExited).Returns(true);
        processMock.Setup(x => x.ExitCode).Returns(0);
        processMock.Setup(x => x.StandardInput).Returns(processInputStream);
        processMock.Setup(x => x.StandardOutput).Throws(new IOException("Read failed"));
        processMock.Setup(x => x.StandardError).Returns(errorStream);
        processMock.Setup(x => x.Id).Returns(12345);

        _processFactoryMock.Setup(x => x.CreateProcess(It.IsAny<TypstCompileOptions>()))
            .Returns(processMock.Object);

        // Act
        var result = await _compiler.CompileAsync(inputStream, compileOptions);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.ToString().Should().Contain("Failed to read stdout stream");

        processMock.Verify(x => x.Dispose(), Times.Once);
    }
}