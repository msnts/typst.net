# Typst.Net

Typst.Net is a .NET library that provides a wrapper around the Typst compiler, allowing you to compile Typst documents to various output formats (PDF, SVG, PNG) directly from your .NET applications.

## Features

- Compile Typst documents to PDF, SVG, or PNG formats
- Support for custom font paths
- Configurable root directory for resolving relative paths
- Support for multiple input files
- Asynchronous compilation with cancellation support
- Dependency injection support through Microsoft.Extensions.DependencyInjection

## Installation

You can install the package via NuGet:

```bash
dotnet add package Typst.Net.Core
```

## Usage

### Basic Usage

```csharp
using Typst.Net.Core;

// Create compiler options
var options = new TypstCompileOptions
{
    Format = OutputFormat.Pdf,
    RootDirectory = "/path/to/your/project",
    FontPaths = new[] { "/path/to/fonts" }
};

// Create the compiler
var compiler = new TypstCompiler();

// Compile a document
using var inputStream = File.OpenRead("document.typ");
var result = await compiler.CompileAsync(inputStream, options);

// Access the compiled output
var pdfBytes = result.Output;
```

### Using Dependency Injection

```csharp
using Microsoft.Extensions.DependencyInjection;
using Typst.Net.Core;

// Configure services
var services = new ServiceCollection();
services.AddTypstCompiler();

// Get the compiler
var serviceProvider = services.BuildServiceProvider();
var compiler = serviceProvider.GetRequiredService<ITypstCompiler>();
```

## Configuration Options

The `TypstCompileOptions` class provides several configuration options:

- `Format`: The desired output format (PDF, SVG, or PNG)
- `RootDirectory`: The root directory for resolving relative paths
- `FontPaths`: Collection of font file paths to be used during compilation
- `Inputs`: Dictionary of input files and their content

## Requirements

- .NET 6.0 or later
- Typst compiler installed on the system

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details. 