# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- **Font Awesome Integration**: Migrated from Unicode emojis to Font Awesome Solid/Regular fonts with `IconGlyphs` helper class
- **Enhanced MiniTaskbar UI**:
  - Enlarged clock display (140px width, 32pt font) for improved visibility
  - Added progress bar time overlay (14px height) showing remaining Pomodoro time
  - Removed Close button to streamline interface
- **Paths Configuration**:
  - Added `DailyNotesFolder` configuration option
  - Added `TemplatesFolder` configuration option
  - Extended Settings page with Paths section (Inbox, Daily Notes, Templates)
- **System Tray Icon**: Re-enabled TrayIconService with contextual menu (Start Pomodoro, Settings, Quit)
- **Build Automation**:
  - Cake build system with `GenerateTrayIcon` task
  - PowerShell bootstrapper (`build.ps1`) for Cake execution
  - Python script integration for icon generation
- **VS Code Workspace Configuration**:
  - Comprehensive `.vscode/settings.json` with C# and Python settings
  - Extension recommendations in `.vscode/extensions.json` (C# Dev Kit, Python, Cake, GitLens, SonarLint, etc.)
  - Debug configurations in `.vscode/launch.json` for .NET MAUI and Python
  - Build/test/clean tasks in `.vscode/tasks.json`
- **Production Readiness Tooling**:
  - `.editorconfig` with comprehensive C# and Python code style rules
  - MIT License (`LICENSE` file)
  - Code analyzers enabled in `.csproj` (EnableNETAnalyzers, AnalysisLevel=latest-all, EnforceCodeStyleInBuild)
  - GitHub Actions CI/CD workflow (`.github/workflows/ci.yml`) with build/test/coverage/CodeQL security scanning
  - Dependabot configuration (`.github/dependabot.yml`) for automated dependency updates (NuGet, pip, GitHub Actions)
- **Developer Documentation**:
  - `CONTRIBUTING.md` with contribution guidelines, coding standards, testing requirements, PR process
  - `CHANGELOG.md` for version history tracking

### Changed

- **Solution Format**: Migrated from `.sln` to `.slnx` (XML-based solution format)
- **Target Framework**: Simplified to single `net9.0-windows10.0.19041.0` target
- **MiniTaskbar Layout**: Moved ExpandedContent to Grid.Row 3, optimized for 275px collapsed width
- **PathSettings Model**: Extended with `DailyNotesFolder` and `TemplatesFolder` properties, added backward-compatible `DailyNotesPath` computed property
- **TrayIconService**: Updated `GetApplicationIcon()` to load `Resources/Images/trayicon.ico`

### Fixed

- MiniTaskbar collapsed width constraint (now properly fits within 275px)
- Cake build script syntax errors (removed triple-slash comments)
- Dependabot configuration lint errors (removed unsupported `reviewers` property)

### Removed

- Close button from MiniTaskbar window
- CornerRadius property from ProgressBar (not supported in .NET MAUI)

## [0.1.0] - Initial Development

### Features

- Core ADHD workspace management features:
  - Focus Tracker with window activity monitoring
  - Pomodoro timer (50-min focus, 10-min break, 30-min long break)
  - Quick Capture for rapid note-taking
  - Context restore for workspace state management
- Obsidian integration for note management
- Multi-screen support with per-monitor workspace layouts
- Windows hotkey support for quick actions
- Configuration system with JSON persistence
- Comprehensive logging with Serilog
- Basic unit tests for configuration and commands

[Unreleased]: https://github.com/ballombert/NeuroNest/compare/v0.1.0...HEAD
[0.1.0]: https://github.com/ballombert/NeuroNest/releases/tag/v0.1.0
