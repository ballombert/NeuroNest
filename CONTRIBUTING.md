# Contributing to NeuroNest (ADHD Workspace)

First off, thank you for considering contributing to NeuroNest! 🎉

This document provides guidelines for contributing to this project. Following these guidelines helps communicate that you respect the time of the developers managing and developing this open-source project.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Development Setup](#development-setup)
- [How to Contribute](#how-to-contribute)
- [Coding Standards](#coding-standards)
- [Testing Guidelines](#testing-guidelines)
- [Commit Message Guidelines](#commit-message-guidelines)
- [Pull Request Process](#pull-request-process)

## Code of Conduct

This project adheres to a code of conduct. By participating, you are expected to uphold this code. Please report unacceptable behavior to the project maintainers.

## Getting Started

### Prerequisites

- Windows 10 (build 19041) or Windows 11
- .NET 9 SDK
- Visual Studio 2022 or VS Code with C# Dev Kit
- Python 3.7+ (for build scripts)
- Pillow library (`pip install pillow`)

### First Contribution

Looking for a place to start? Check out issues labeled with:
- `good first issue` - Good for newcomers
- `help wanted` - Extra attention needed
- `documentation` - Documentation improvements

## Development Setup

1. **Clone the repository**
   ```powershell
   git clone https://github.com/ballombert/NeuroNest.git
   cd NeuroNest
   ```

2. **Restore dependencies**
   ```powershell
   dotnet restore
   ```

3. **Build the project**
   ```powershell
   dotnet build
   ```

4. **Run tests**
   ```powershell
   dotnet test
   ```

5. **Run the application**
   ```powershell
   dotnet run
   ```

### VS Code Setup

The repository includes a `.vscode` folder with recommended settings and extensions. Install the recommended extensions when prompted.

## How to Contribute

### Reporting Bugs

Before creating bug reports, please check existing issues to avoid duplicates. When creating a bug report, include:

- **Clear title and description**
- **Steps to reproduce**
- **Expected vs actual behavior**
- **Screenshots** (if applicable)
- **Environment details** (OS version, .NET version)
- **Relevant logs** from `C:\Temp\adhd-workspace-*.log`

### Suggesting Enhancements

Enhancement suggestions are tracked as GitHub issues. When creating an enhancement suggestion, include:

- **Clear title and description**
- **Rationale** - Why is this enhancement useful?
- **Proposed solution**
- **Alternative solutions** considered
- **Additional context** (mockups, examples)

### Code Contributions

1. **Fork** the repository
2. **Create a branch** (`git checkout -b feature/amazing-feature`)
3. **Make your changes**
4. **Add tests** for new functionality
5. **Ensure tests pass** (`dotnet test`)
6. **Commit** your changes (see commit guidelines below)
7. **Push** to your fork (`git push origin feature/amazing-feature`)
8. **Open a Pull Request**

## Coding Standards

This project follows standard C# conventions defined in `.editorconfig`. Key points:

### General
- Use **4 spaces** for indentation (not tabs)
- Use **PascalCase** for class names, methods, properties
- Use **camelCase** for local variables, parameters
- Use **_camelCase** for private fields
- Prefix interfaces with **I** (e.g., `IConfigurationService`)

### C# Style
- Use `var` when type is obvious
- Use nullable reference types (`#nullable enable`)
- Prefer expression-bodied members for simple properties
- Use pattern matching where appropriate
- Follow async/await best practices:
  - Avoid `async void` (use `async Task` instead)
  - Use `ConfigureAwait(false)` in library code
  - Name async methods with `Async` suffix

### XAML Style
- Use **4 spaces** for indentation
- Group properties logically (layout, style, behavior)
- Use StaticResource bindings when possible
- Add `x:Name` only when needed in code-behind

### Documentation
- Add XML comments for public APIs
- Include `<summary>`, `<param>`, `<returns>` tags
- Document exceptions with `<exception>` tags

Example:
```csharp
/// <summary>
/// Starts a Pomodoro focus session with the specified duration.
/// </summary>
/// <param name="durationMinutes">Duration in minutes (default: 50)</param>
/// <returns>A task representing the async operation</returns>
/// <exception cref="InvalidOperationException">Thrown if a session is already running</exception>
public async Task StartSessionAsync(int durationMinutes = 50)
{
    // Implementation
}
```

## Testing Guidelines

### Unit Tests
- Use **xUnit** framework
- Use **Moq** for mocking dependencies
- Follow **AAA pattern** (Arrange, Act, Assert)
- One assertion per test (when possible)
- Use descriptive test names: `MethodName_Scenario_ExpectedBehavior`

Example:
```csharp
[Fact]
public void SaveSettings_WhenPathsAreValid_ShouldSaveSuccessfully()
{
    // Arrange
    var service = new ConfigurationService(mockLogger.Object);
    var settings = new AppSettings { /* ... */ };

    // Act
    var result = service.Save(settings);

    // Assert
    Assert.True(result);
}
```

### Integration Tests
- Test service interactions
- Use realistic test data
- Clean up resources in `Dispose()`

### Coverage
- Aim for **60%+ code coverage**
- Focus on business logic and services
- UI code coverage is optional

Run coverage reports:
```powershell
dotnet test --collect:"XPlat Code Coverage"
```

## Commit Message Guidelines

We follow **Conventional Commits** specification:

```
<type>(<scope>): <subject>

<body>

<footer>
```

### Types
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `style`: Code style changes (formatting, no logic change)
- `refactor`: Code refactoring
- `perf`: Performance improvements
- `test`: Adding or updating tests
- `build`: Build system changes
- `ci`: CI configuration changes
- `chore`: Other changes (dependencies, etc.)

### Examples
```
feat(pomodoro): add long break after 4 cycles

Implemented automatic long break (30min) after completing 4 focus cycles.
Configurable via Settings > Pomodoro > Cycles Before Long Break.

Closes #42
```

```
fix(ui): correct progress bar visibility in collapsed mode

Progress bar now properly displays time remaining with 14px height.
Removed unsupported CornerRadius property causing build errors.

Fixes #89
```

## Pull Request Process

1. **Update documentation** if you changed APIs or features
2. **Add tests** for new functionality
3. **Ensure CI passes** (build, tests, code analysis)
4. **Update CHANGELOG.md** with your changes
5. **Link related issues** in PR description
6. **Request review** from maintainers

### PR Title Format
Use conventional commit format:
```
feat(scope): brief description
```

### PR Description Template
```markdown
## Description
Brief description of changes

## Motivation
Why is this change needed?

## Changes Made
- Change 1
- Change 2

## Testing
How was this tested?

## Screenshots (if applicable)

## Checklist
- [ ] Tests added/updated
- [ ] Documentation updated
- [ ] CHANGELOG.md updated
- [ ] CI passes
- [ ] Follows coding standards
```

## Questions?

Feel free to:
- Open an issue with the `question` label
- Join discussions in existing issues
- Reach out to maintainers

Thank you for contributing! 🚀
