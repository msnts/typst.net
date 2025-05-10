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

Typst.Net is a .NET library that provides a wrapper around the Typst compiler, allowing you to compile Typst documents to various output formats (PDF, SVG, PNG) directly from your .NET applications.

## ✨ Features

- Compile Typst documents to PDF, SVG, or PNG formats
- Support for custom font paths
- Configurable root directory for resolving relative paths
- Asynchronous compilation with cancellation support
- Dependency injection integration through Microsoft.Extensions.DependencyInjection
- Cross-platform support (Windows, Linux, macOS)

## 🚀 Installation

You can install the package via NuGet:

```bash
dotnet add package Typst.Net
```

## 🔧 Prerequisites

- .NET 6.0 or later
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
services.AddTypst();

// Get the compiler
var serviceProvider = services.BuildServiceProvider();
var compiler = serviceProvider.GetRequiredService<ITypstCompiler>();
```

### Advanced Usage with Multiple Inputs

```csharp
var options = new TypstCompileOptions
{
    Format = OutputFormat.Pdf,
    RootDirectory = "/path/to/your/project",
    FontPaths = new[] { "/path/to/fonts" },
    Inputs = new Dictionary<string, Stream>
    {
        ["main.typ"] = File.OpenRead("main.typ"),
        ["template.typ"] = File.OpenRead("template.typ")
    }
};

var compiler = new TypstCompiler();
var result = await compiler.CompileAsync(options);
```

## ⚙️ Configuration Options

The `TypstCompileOptions` class provides several configuration options:

| Option | Type | Description |
|--------|------|-------------|
| `Format` | `OutputFormat` | The desired output format (PDF, SVG, or PNG) |
| `RootDirectory` | `string` | The root directory for resolving relative paths |
| `FontPaths` | `string[]` | Collection of font file paths to be used during compilation |
| `Inputs` | `Dictionary<string, Stream>` | Dictionary of input files and their content |

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