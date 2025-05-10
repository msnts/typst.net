# Contributing to Typst.Net

First of all, thank you for considering contributing to Typst.Net! It's people like you that make this project such a great tool.

This document provides guidelines and steps for contributing to Typst.Net. Please take a moment to review this document in order to make the contribution process easy and effective for everyone involved.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
  - [Environment Setup](#environment-setup)
  - [Building the Project](#building-the-project)
  - [Running Tests](#running-tests)
- [Development Workflow](#development-workflow)
  - [Branching Strategy](#branching-strategy)
  - [Commit Guidelines](#commit-guidelines)
  - [Pull Request Process](#pull-request-process)
- [Coding Standards](#coding-standards)
- [Documentation](#documentation)
- [Reporting Bugs](#reporting-bugs)
- [Feature Requests](#feature-requests)
- [Release Process](#release-process)

## Code of Conduct

This project and everyone participating in it is governed by the [Typst.Net Code of Conduct](CODE_OF_CONDUCT.md). By participating, you are expected to uphold this code. Please report unacceptable behavior to [project.maintainer@example.com](mailto:project.maintainer@example.com).

## Getting Started

### Environment Setup

1. Ensure you have the following installed:
   - .NET 6.0 SDK or later
   - Visual Studio 2022, Visual Studio Code, or JetBrains Rider
   - Git
   - Typst compiler (see README.md for installation instructions)

2. Fork the repository on GitHub.

3. Clone your fork locally:
   ```
   git clone https://github.com/yourusername/Typst.Net.git
   cd Typst.Net
   ```

4. Add the upstream repository as a remote:
   ```
   git remote add upstream https://github.com/originalmaintainer/Typst.Net.git
   ```

### Building the Project

Run the following command in the root directory:
```
dotnet build
```

### Running Tests

To run tests, execute:
```
dotnet test
```

## Development Workflow

### Branching Strategy

We use GitHub Flow, a lightweight, branch-based workflow:

- `main`: The main branch contains stable, deployable code that is released to NuGet.
- All development happens in feature branches created directly from `main`.

For new features or bug fixes:

1. Create a branch from `main`:
   ```
   git checkout main
   git pull upstream main
   git checkout -b feature/your-feature-name
   ```

2. Develop your changes in your feature branch.

### Commit Guidelines

We follow the [Conventional Commits](https://www.conventionalcommits.org/) specification for commit messages. The commit message should be structured as follows:

```
<type>(<scope>): <description>

[optional body]

[optional footer(s)]
```

Types include:
- `feat`: A new feature
- `fix`: A bug fix
- `docs`: Documentation only changes
- `style`: Changes that don't affect the meaning of the code
- `refactor`: A code change that neither fixes a bug nor adds a feature
- `perf`: A code change that improves performance
- `test`: Adding missing tests or correcting existing tests
- `chore`: Changes to the build process or auxiliary tools and libraries

Example:
```
feat(compiler): add support for SVG output format

Added SVG output format to the TypstCompiler class, allowing users to compile
Typst documents to SVG format in addition to PDF and PNG.

Resolves #123
```

### Pull Request Process

1. Update your feature branch with the latest changes from the `main` branch if needed:
   ```
   git checkout main
   git pull upstream main
   git checkout feature/your-feature-name
   git rebase main
   ```

2. Push your branch to your fork:
   ```
   git push origin feature/your-feature-name
   ```

3. Submit a pull request to the `main` branch of the original repository.

4. Add a clear description to your pull request, including:
   - The purpose of the changes
   - Links to any related issues (Use keywords like "Fixes #123" to automatically close issues when the PR is merged)
   - Any breaking changes
   - Any additional information that may be helpful

5. Wait for a review. Make any requested changes and push them to your branch.

6. Once approved, your PR will be merged by a maintainer.

7. After merging, delete your feature branch both locally and on your fork.

## Coding Standards

We follow the [.NET Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions) and enforce them through `.editorconfig`. Key points:

- Use PascalCase for class names, method names, and properties
- Use camelCase for local variables and parameters
- Use underscore prefix for private fields: `_privateField`
- Use braces for all control structures, even single-line ones
- Keep lines to a reasonable length (120 characters)
- Use meaningful variable and method names
- Include XML documentation comments for public APIs

## Documentation

- All public APIs should be documented with XML comments
- Update the README.md when adding new features
- Add code examples for new features when possible
- Keep documentation up-to-date with code changes

## Reporting Bugs

When reporting bugs, please include:

1. A clear and descriptive title
2. Steps to reproduce the issue
3. Expected behavior
4. Actual behavior
5. Environment details (OS, .NET version, etc.)
6. Any relevant logs or screenshots

Use the issue template provided in the repository when creating a new issue.

## Feature Requests

For feature requests, please include:

1. A clear and descriptive title
2. A detailed description of the proposed feature
3. Any relevant examples or use cases
4. Any potential implementation ideas you may have

Use the feature request template provided in the repository when creating a new feature request.

## Release Process

1. The maintainer will create a release directly from `main` when ready
2. Version numbers will be updated and release notes finalized
3. The commit will be tagged with the version number (e.g., `v1.2.0`)
4. GitHub Release will be created from this tag
5. The new version will be published to NuGet

Releases follow [Semantic Versioning](https://semver.org/) (MAJOR.MINOR.PATCH):
- MAJOR version for incompatible API changes
- MINOR version for backward-compatible functionality additions
- PATCH version for backward-compatible bug fixes

Thank you for contributing to Typst.Net!