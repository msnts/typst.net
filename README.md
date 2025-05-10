# Typst.Net

<div align="center">

[![NuGet](https://img.shields.io/nuget/v/Typst.Net.Core.svg)](https://www.nuget.org/packages/Typst.Net.Core)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Build Status](https://github.com/msnts/Typst.Net/actions/workflows/ci.yml/badge.svg)]()

</div>

<div align="center">

[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=msnts_typst.net&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=msnts_typst.net)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=msnts_typst.net&metric=coverage)](https://sonarcloud.io/summary/new_code?id=msnts_typst.net)
[![Bugs](https://sonarcloud.io/api/project_badges/measure?project=msnts_typst.net&metric=bugs)](https://sonarcloud.io/summary/new_code?id=msnts_typst.net)
[![Code Smells](https://sonarcloud.io/api/project_badges/measure?project=msnts_typst.net&metric=code_smells)](https://sonarcloud.io/summary/new_code?id=msnts_typst.net)
[![Duplicated Lines (%)](https://sonarcloud.io/api/project_badges/measure?project=msnts_typst.net&metric=duplicated_lines_density)](https://sonarcloud.io/summary/new_code?id=msnts_typst.net)

</div>

## 📖 About

Typst.Net is a .NET library that provides a wrapper around the Typst compiler, allowing you to compile Typst documents to various output formats (PDF, SVG, PNG) directly from your .NET applications. It offers a robust, asynchronous API with comprehensive error handling and logging capabilities.

## ✨ Features

- Compile Typst documents to PDF, SVG, or PNG formats
- Support for custom font paths
- Configurable root directory for resolving relative paths
- Asynchronous compilation with cancellation support
- Comprehensive error handling and logging
- Dependency injection integration through Microsoft.Extensions.DependencyInjection
- Cross-platform support (Windows, Linux, macOS)
- Process management and resource cleanup
- Configurable Typst executable path

## 🚀 Installation

You can install the package via NuGet:

```bash
dotnet add package Typst.Net
```

## 🔧 Prerequisites

- .NET 8.0 or later
- Typst compiler installed on the system

### Installing Typst

#### Windows
```powershell
winget install typst
```

#### macOS
```bash
brew install typst
```

#### Linux
```bash
cargo install typst-cli
```

## 📝 Usage

### Basic Usage

```csharp
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Typst.Net.Core;
using Typst.Net.Core.Configuration;
using Typst.Net.Core.Process;

// Configure Typst options
var typstOptions = new TypstOptions
{
    ExecutablePath = "/path/to/typst" // e.g., "typst" if it's in PATH
};

// Create logger (using console logger for example)
using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<TypstCompiler>();

// Create process factory
var processFactory = new TypstProcessFactory(
    Options.Create(typstOptions),
    loggerFactory.CreateLogger<TypstProcessFactory>()
);

// Create compiler options
var options = new TypstCompileOptions
{
    Format = OutputFormat.Pdf,
    RootDirectory = "/path/to/your/project",
    FontPaths = new[] { "/path/to/fonts" }
};

// Create the compiler
var compiler = new TypstCompiler(logger, processFactory);

// Compile a document
using var inputStream = File.OpenRead("document.typ");
var result = await compiler.CompileAsync(inputStream, options);

// Access the compiled output
var pdfBytes = result.OutputData;
var stdErr = result.StandardError;
```

### Using Dependency Injection

```csharp
using Microsoft.Extensions.DependencyInjection;
using Typst.Net.Core;

// Configure services
var services = new ServiceCollection();

// Configure Typst options
services.Configure<TypstOptions>(options =>
{
    options.ExecutablePath = "/path/to/typst";
});

// Add Typst services
services.AddTypst();

// Get the compiler
var serviceProvider = services.BuildServiceProvider();
var compiler = serviceProvider.GetRequiredService<ITypstCompiler>();
```

### Error Handling

The library provides several exception types for different error scenarios:

- `TypstCompilationException`: Thrown when the Typst compilation fails
- `TypstProcessException`: Thrown when there are issues with the Typst process
- `TypstConfigurationException`: Thrown when there are configuration issues

Example error handling:

```csharp
try
{
    var result = await compiler.CompileAsync(inputStream, options);
    // Process successful result
}
catch (TypstCompilationException ex)
{
    // Handle compilation errors
    Console.WriteLine($"Compilation failed: {ex.Message}");
    Console.WriteLine($"Standard error: {ex.StandardError}");
}
catch (TypstProcessException ex)
{
    // Handle process-related errors
    Console.WriteLine($"Process error: {ex.Message}");
}
catch (TypstConfigurationException ex)
{
    // Handle configuration errors
    Console.WriteLine($"Configuration error: {ex.Message}");
}
```

## ⚙️ Configuration Options

### TypstCompileOptions

| Option | Type | Description |
|--------|------|-------------|
| `Format` | `OutputFormat` | The desired output format (PDF, SVG, or PNG) |
| `RootDirectory` | `string` | The root directory for resolving relative paths |
| `FontPaths` | `IEnumerable<string>` | Collection of font file paths to be used during compilation |
| `Data` | `string` | Additional data to be included in the compilation process |

### TypstOptions

| Option | Type | Description |
|--------|------|-------------|
| `ExecutablePath` | `string` | The path to the Typst executable |

## 🛠️ Example Projects

For practical examples of how to use Typst.Net in your projects, check out the [examples](./examples) folder in the repository.

## 👥 Contributing

Contributions are welcome! Please feel free to submit a Pull Request. For major changes, please open an issue first to discuss what you would like to change.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

See [CONTRIBUTING.md](CONTRIBUTING.md) for more details about our contribution process.

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 📊 Project Statistics

[![](https://img.shields.io/github/issues/msnts/Typst.Net)](https://github.com/msnts/Typst.Net/issues)
[![](https://img.shields.io/github/stars/msnts/Typst.Net)](https://github.com/msnts/Typst.Net/stargazers)
[![](https://img.shields.io/github/forks/msnts/Typst.Net)](https://github.com/msnts/Typst.Net/network/members)