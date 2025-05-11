using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Typst.Net.Core.Configuration;
using Typst.Net.Core.Process;

namespace Typst.Net.Core.Tests;

public sealed class TypstProcessFactoryTests : IDisposable
{
    private readonly Mock<ILogger<TypstProcessFactory>> _loggerMock;
    private readonly Mock<IOptions<TypstOptions>> _optionsMock;
    private readonly List<IDisposable> _disposables = [];

    public TypstProcessFactoryTests()
    {
        _loggerMock = new Mock<ILogger<TypstProcessFactory>>();
        _optionsMock = new Mock<IOptions<TypstOptions>>();
        _optionsMock.Setup(x => x.Value).Returns(new TypstOptions { ExecutablePath = "typst" });
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
        // Act & Assert
        var act = () => new TypstProcessFactory(null!, _loggerMock.Object);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("options");
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => new TypstProcessFactory(_optionsMock.Object, null!);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    [Fact]
    public void Constructor_WithNullOptionsValue_ThrowsArgumentNullException()
    {
        // Arrange
        var optionsMock = new Mock<IOptions<TypstOptions>>();
        optionsMock.Setup(x => x.Value).Returns((TypstOptions)null!);

        // Act & Assert
        var act = () => new TypstProcessFactory(optionsMock.Object, _loggerMock.Object);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("options");
    }

    [Fact]
    public void CreateProcess_WithNullCompileOptions_ThrowsArgumentNullException()
    {
        // Arrange
        var factory = new TypstProcessFactory(_optionsMock.Object, _loggerMock.Object);

        // Act & Assert
        var act = () => factory.CreateProcess(null!);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("compileOptions");
    }

    [Theory]
    [InlineData(OutputFormat.Pdf)]
    [InlineData(OutputFormat.Png)]
    [InlineData(OutputFormat.Svg)]
    public void CreateProcess_WithDifferentFormats_CreatesProcessWithCorrectArguments(OutputFormat format)
    {
        // Arrange
        var factory = new TypstProcessFactory(_optionsMock.Object, _loggerMock.Object);
        var compileOptions = new TypstCompileOptions { Format = format };

        // Act
        var process = factory.CreateProcess(compileOptions);
        _disposables.Add(process);

        // Assert
        process.Should().NotBeNull();
        process.Should().BeAssignableTo<ITypstProcess>();

        var processInfo = process.StartInfo;
        processInfo.Should().NotBeNull();
        processInfo.FileName.Should().Be("typst");
        processInfo.Arguments.Should().Contain($"--format {format.ToString().ToLowerInvariant()}");
    }

    [Fact]
    public void CreateProcess_WithRootDirectory_CreatesProcessWithCorrectWorkingDirectory()
    {
        // Arrange
        var factory = new TypstProcessFactory(_optionsMock.Object, _loggerMock.Object);
        const string rootDir = "/custom/root/dir";
        var compileOptions = new TypstCompileOptions
        {
            Format = OutputFormat.Pdf,
            RootDirectory = rootDir
        };

        // Act
        var process = factory.CreateProcess(compileOptions);
        _disposables.Add(process);

        // Assert
        var processInfo = process.StartInfo;
        processInfo.WorkingDirectory.Should().Be(rootDir);
        processInfo.Arguments.Should().Contain($"--root \"{rootDir}\"");
    }

    [Fact]
    public void CreateProcess_WithFontPaths_CreatesProcessWithCorrectFontArguments()
    {
        // Arrange
        var factory = new TypstProcessFactory(_optionsMock.Object, _loggerMock.Object);
        var fontPaths = new[] { "/fonts/font1.ttf", "/fonts/font2.ttf" };
        var compileOptions = new TypstCompileOptions
        {
            Format = OutputFormat.Pdf,
            FontPaths = fontPaths
        };

        // Act
        var process = factory.CreateProcess(compileOptions);
        _disposables.Add(process);

        // Assert
        var processInfo = process.StartInfo;
        processInfo.Arguments.Should().Contain("--font-path \"/fonts/font1.ttf\"");
        processInfo.Arguments.Should().Contain("--font-path \"/fonts/font2.ttf\"");
    }

    [Fact]
    public void CreateProcess_WithData_CreatesProcessWithCorrectDataArgument()
    {
        // Arrange
        var factory = new TypstProcessFactory(_optionsMock.Object, _loggerMock.Object);
        const string data = "test data";
        var compileOptions = new TypstCompileOptions
        {
            Format = OutputFormat.Pdf,
            Data = data
        };

        // Act
        var process = factory.CreateProcess(compileOptions);
        _disposables.Add(process);

        // Assert
        var processInfo = process.StartInfo;
        processInfo.Arguments.Should().Contain($"--input data={data}");
    }

    [Fact]
    public void CreateProcess_WithEmptyFontPaths_DoesNotIncludeFontArguments()
    {
        // Arrange
        var factory = new TypstProcessFactory(_optionsMock.Object, _loggerMock.Object);
        var compileOptions = new TypstCompileOptions
        {
            Format = OutputFormat.Pdf,
            FontPaths = []
        };

        // Act
        var process = factory.CreateProcess(compileOptions);
        _disposables.Add(process);

        // Assert
        var processInfo = process.StartInfo;
        processInfo.Arguments.Should().NotContain("--font-path");
    }

    [Fact]
    public void CreateProcess_WithNullFontPaths_DoesNotIncludeFontArguments()
    {
        // Arrange
        var factory = new TypstProcessFactory(_optionsMock.Object, _loggerMock.Object);
        var compileOptions = new TypstCompileOptions
        {
            Format = OutputFormat.Pdf,
            FontPaths = null
        };

        // Act
        var process = factory.CreateProcess(compileOptions);
        _disposables.Add(process);

        // Assert
        var processInfo = process.StartInfo;
        processInfo.Arguments.Should().NotContain("--font-path");
    }

    [Fact]
    public void CreateProcess_WithEmptyData_DoesNotIncludeDataArgument()
    {
        // Arrange
        var factory = new TypstProcessFactory(_optionsMock.Object, _loggerMock.Object);
        var compileOptions = new TypstCompileOptions
        {
            Format = OutputFormat.Pdf,
            Data = string.Empty
        };

        // Act
        var process = factory.CreateProcess(compileOptions);
        _disposables.Add(process);

        // Assert
        var processInfo = process.StartInfo;
        processInfo.Arguments.Should().NotContain("--input data=");
    }

    [Fact]
    public void CreateProcess_WithNullRootDirectory_UsesCurrentDirectory()
    {
        // Arrange
        var factory = new TypstProcessFactory(_optionsMock.Object, _loggerMock.Object);
        var compileOptions = new TypstCompileOptions
        {
            Format = OutputFormat.Pdf,
            RootDirectory = null
        };

        // Act
        var process = factory.CreateProcess(compileOptions);
        _disposables.Add(process);

        // Assert
        var processInfo = process.StartInfo;
        processInfo.WorkingDirectory.Should().Be(Environment.CurrentDirectory);
        processInfo.Arguments.Should().NotContain("--root");
    }

    [Fact]
    public void CreateProcess_ConfiguresProcessStartInfoCorrectly()
    {
        // Arrange
        var factory = new TypstProcessFactory(_optionsMock.Object, _loggerMock.Object);
        var compileOptions = new TypstCompileOptions { Format = OutputFormat.Pdf };

        // Act
        var process = factory.CreateProcess(compileOptions);
        _disposables.Add(process);

        // Assert
        var processInfo = process.StartInfo;
        processInfo.Should().NotBeNull();
        processInfo.FileName.Should().Be("typst");
        processInfo.RedirectStandardInput.Should().BeTrue();
        processInfo.RedirectStandardOutput.Should().BeTrue();
        processInfo.RedirectStandardError.Should().BeTrue();
        processInfo.UseShellExecute.Should().BeFalse();
        processInfo.CreateNoWindow.Should().BeTrue();
        processInfo.StandardOutputEncoding.Should().BeNull();
        processInfo.StandardErrorEncoding.Should().NotBeNull();
    }
}