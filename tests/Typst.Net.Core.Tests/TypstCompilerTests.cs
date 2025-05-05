using System.Diagnostics;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Typst.Net.Core.Configuration;
using Typst.Net.Core.Exceptions;

namespace Typst.Net.Core.Tests;

public sealed class TypstCompilerTests : IDisposable
{
    private readonly Mock<IProcessWrapper> _processWrapperMock;
    private readonly Mock<ILogger<TypstCompiler>> _loggerMock;
    private readonly Mock<IOptions<TypstOptions>> _optionsMock;
    private readonly TypstCompiler _compiler;
    private readonly List<IDisposable> _disposables = new();

    public TypstCompilerTests()
    {
        _processWrapperMock = new Mock<IProcessWrapper>();
        _loggerMock = new Mock<ILogger<TypstCompiler>>();
        _optionsMock = new Mock<IOptions<TypstOptions>>();

        _optionsMock.Setup(x => x.Value)
            .Returns(new TypstOptions { ExecutablePath = "typst" });

        _compiler = new TypstCompiler(
            _optionsMock.Object,
            _loggerMock.Object,
            _processWrapperMock.Object);
    }

    public void Dispose()
    {
        foreach (var disposable in _disposables)
        {
            disposable.Dispose();
        }
    }

    [Fact]
    public void Constructor_WithNullOptions_ThrowsArgumentNullException()
    {
        // Arrange
        var logger = Mock.Of<ILogger<TypstCompiler>>();
        var processWrapper = Mock.Of<IProcessWrapper>();

        // Act & Assert
        var act = () => new TypstCompiler(null!, logger, processWrapper);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("options");
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange
        var processWrapper = Mock.Of<IProcessWrapper>();

        // Act & Assert
        var act = () => new TypstCompiler(_optionsMock.Object, null!, processWrapper);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    [Fact]
    public void Constructor_WithNullProcessWrapper_ThrowsArgumentNullException()
    {
        // Arrange
        var logger = Mock.Of<ILogger<TypstCompiler>>();

        // Act & Assert
        var act = () => new TypstCompiler(_optionsMock.Object, logger, null!);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("processWrapper");
    }

    [Fact]
    public async Task CompileAsync_WithNullInputStream_ThrowsArgumentNullException()
    {
        // Arrange
        var compileOptions = new TypstCompileOptions();

        // Act & Assert
        var act = () => _compiler.CompileAsync(null!, compileOptions);
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("inputStream");
    }

    [Fact]
    public async Task CompileAsync_WithNullCompileOptions_ThrowsArgumentNullException()
    {
        // Arrange
        var inputStream = new MemoryStream();
        _disposables.Add(inputStream);

        // Act & Assert
        var act = () => _compiler.CompileAsync(inputStream, null!);
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("compileOptions");
    }

    [Fact]
    public async Task CompileAsync_WithNonReadableStream_ThrowsArgumentException()
    {
        // Arrange
        var inputStream = new MemoryStream();
        inputStream.Close();
        _disposables.Add(inputStream);

        var compileOptions = new TypstCompileOptions();

        // Act & Assert
        var act = () => _compiler.CompileAsync(inputStream, compileOptions);
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("inputStream")
            .WithMessage("Input stream must be readable.*");
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

        var processMock = new Mock<IProcess>();
        processMock.Setup(x => x.Start()).Returns(true);
        processMock.Setup(x => x.HasExited).Returns(true);
        processMock.Setup(x => x.ExitCode).Returns(0);
        processMock.Setup(x => x.StandardInput).Returns(processInputStream);
        processMock.Setup(x => x.StandardOutput).Returns(outputStream);
        processMock.Setup(x => x.StandardError).Returns(errorStream);
        processMock.Setup(x => x.Id).Returns(12345);

        _processWrapperMock.Setup(x => x.CreateProcess(It.IsAny<ProcessStartInfo>()))
            .Returns(processMock.Object);

        // Act
        var result = await _compiler.CompileAsync(inputStream, compileOptions);

        // Assert
        result.Should().NotBeNull();
        result.OutputData.Should().NotBeNull();

        _processWrapperMock.Verify(x => x.CreateProcess(It.Is<ProcessStartInfo>(p => 
            p.FileName == "typst" && 
            p.Arguments.Contains("--format pdf"))), Times.Once);

        processMock.Verify(x => x.Start(), Times.Once);
        processMock.Verify(x => x.WaitForExitAsync(It.IsAny<CancellationToken>()), Times.Once);
        processMock.Verify(x => x.Dispose(), Times.Once);
    }

    [Fact]
    public async Task CompileAsync_WhenProcessFails_ThrowsTypstCompilationException()
    {
        // Arrange
        var inputStream = new MemoryStream(Encoding.UTF8.GetBytes("Hello, Typst!"));
        _disposables.Add(inputStream);

        var compileOptions = new TypstCompileOptions { Format = OutputFormat.Pdf };
        var errorStream = new MemoryStream(Encoding.UTF8.GetBytes("Compilation error"));
        _disposables.Add(errorStream);

        var processInputStream = new MemoryStream();
        _disposables.Add(processInputStream);

        var processMock = new Mock<IProcess>();
        processMock.Setup(x => x.Start()).Returns(true);
        processMock.Setup(x => x.HasExited).Returns(true);
        processMock.Setup(x => x.ExitCode).Returns(1);
        processMock.Setup(x => x.StandardInput).Returns(processInputStream);
        processMock.Setup(x => x.StandardError).Returns(errorStream);
        processMock.Setup(x => x.Id).Returns(12345);

        _processWrapperMock.Setup(x => x.CreateProcess(It.IsAny<ProcessStartInfo>()))
            .Returns(processMock.Object);

        // Act & Assert
        var act = () => _compiler.CompileAsync(inputStream, compileOptions);
        await act.Should().ThrowAsync<TypstCompilationException>()
            .WithMessage("Typst compilation failed (PID:*");

        processMock.Verify(x => x.Dispose(), Times.Once);
    }

    [Fact]
    public async Task CompileAsync_WhenProcessStartFails_ThrowsTypstProcessException()
    {
        // Arrange
        var inputStream = new MemoryStream(Encoding.UTF8.GetBytes("Hello, Typst!"));
        _disposables.Add(inputStream);

        var compileOptions = new TypstCompileOptions { Format = OutputFormat.Pdf };

        var processMock = new Mock<IProcess>();
        processMock.Setup(x => x.Start()).Returns(false);

        _processWrapperMock.Setup(x => x.CreateProcess(It.IsAny<ProcessStartInfo>()))
            .Returns(processMock.Object);

        // Act & Assert
        var act = () => _compiler.CompileAsync(inputStream, compileOptions);
        await act.Should().ThrowAsync<TypstProcessException>()
            .WithMessage("*Failed to start Typst process*");

        processMock.Verify(x => x.Dispose(), Times.Once);
    }

    [Fact]
    public async Task CompileAsync_WhenCancelled_ThrowsOperationCanceledException()
    {
        // Arrange
        var inputStream = new MemoryStream(Encoding.UTF8.GetBytes("Hello, Typst!"));
        _disposables.Add(inputStream);

        var compileOptions = new TypstCompileOptions { Format = OutputFormat.Pdf };
        var cancellationToken = new CancellationToken(true);

        var processMock = new Mock<IProcess>();
        processMock.Setup(x => x.Start()).Returns(true);
        processMock.Setup(x => x.WaitForExitAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());
        processMock.Setup(x => x.Id).Returns(12345);

        _processWrapperMock.Setup(x => x.CreateProcess(It.IsAny<ProcessStartInfo>()))
            .Returns(processMock.Object);

        // Act & Assert
        var act = () => _compiler.CompileAsync(inputStream, compileOptions, cancellationToken);
        await act.Should().ThrowAsync<OperationCanceledException>();

        processMock.Verify(x => x.Kill(), Times.Once);
        processMock.Verify(x => x.Dispose(), Times.Once);
    }

    [Fact]
    public async Task CompileAsync_WhenStdinWriteFails_ThrowsTypstProcessException()
    {
        // Arrange
        var inputStream = new MemoryStream(Encoding.UTF8.GetBytes("Hello, Typst!"));
        _disposables.Add(inputStream);

        var compileOptions = new TypstCompileOptions { Format = OutputFormat.Pdf };

        var processMock = new Mock<IProcess>();
        processMock.Setup(x => x.Start()).Returns(true);
        processMock.Setup(x => x.StandardInput).Throws(new IOException("Write failed"));
        processMock.Setup(x => x.Id).Returns(12345);

        _processWrapperMock.Setup(x => x.CreateProcess(It.IsAny<ProcessStartInfo>()))
            .Returns(processMock.Object);

        // Act & Assert
        var act = () => _compiler.CompileAsync(inputStream, compileOptions);
        await act.Should().ThrowAsync<TypstProcessException>()
            .WithMessage("IOException during stdin write for PID*");

        processMock.Verify(x => x.Dispose(), Times.Once);
    }

    [Fact]
    public async Task CompileAsync_WhenStdoutReadFails_ThrowsTypstProcessException()
    {
        // Arrange
        var inputStream = new MemoryStream(Encoding.UTF8.GetBytes("Hello, Typst!"));
        _disposables.Add(inputStream);

        var compileOptions = new TypstCompileOptions { Format = OutputFormat.Pdf };
        var errorStream = new MemoryStream();
        _disposables.Add(errorStream);

        var processInputStream = new MemoryStream();
        _disposables.Add(processInputStream);

        var processMock = new Mock<IProcess>();
        processMock.Setup(x => x.Start()).Returns(true);
        processMock.Setup(x => x.HasExited).Returns(true);
        processMock.Setup(x => x.ExitCode).Returns(0);
        processMock.Setup(x => x.StandardInput).Returns(processInputStream);
        processMock.Setup(x => x.StandardOutput).Throws(new IOException("Read failed"));
        processMock.Setup(x => x.StandardError).Returns(errorStream);
        processMock.Setup(x => x.Id).Returns(12345);

        _processWrapperMock.Setup(x => x.CreateProcess(It.IsAny<ProcessStartInfo>()))
            .Returns(processMock.Object);

        // Act & Assert
        var act = () => _compiler.CompileAsync(inputStream, compileOptions);
        await act.Should().ThrowAsync<TypstProcessException>()
            .WithMessage("Failed to read stdout stream from PID*");

        processMock.Verify(x => x.Dispose(), Times.Once);
    }
}