# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Development Commands

### Build and Test Commands
- Build: `dotnet build UltraWideScreenShare.WinForms`
- Build (Release): `dotnet build UltraWideScreenShare.WinForms --configuration Release`
- Run locally: `dotnet run --project UltraWideScreenShare.WinForms`
- Publish x64: `dotnet publish UltraWideScreenShare.WinForms -c Release -r win-x64 --self-contained -o ./publish/x64`
- Publish x86: `dotnet publish UltraWideScreenShare.WinForms -c Release -r win-x86 --self-contained -o ./publish/x86`

### Solution Structure
- Main project: `UltraWideScreenShare.WinForms` (.NET 9 Windows Forms application with Single-Project MSIX packaging)
- Solution file: `UltraWideScreenShare2.sln`

## Architecture Overview

This is a Windows desktop application for ultra-wide screen sharing that uses:

### Core Technologies
- .NET 9 with Windows Forms
- Microsoft.Windows.CsWin32 for Win32 API access
- Windows Magnification API for screen capture
- DPI awareness handling
- Single-Project MSIX packaging for modern deployment

### Key Components

#### MainWindow.cs (C:\repos\UltraWideScreenShare-2\UltraWideScreenShare.WinForms\MainWindow.cs)
- Main application window with custom border rendering
- Handles DPI scaling and window positioning
- Manages the magnifier panel for screen capture
- Uses 30fps timer for real-time updates

#### Magnifier.cs (C:\repos\UltraWideScreenShare-2\UltraWideScreenShare.WinForms\Magnifier.cs)
- Wraps Windows Magnification API
- Creates magnifier window as child of host panel
- Handles layered window attributes and message filtering

#### Program.cs (C:\repos\UltraWideScreenShare-2\UltraWideScreenShare.WinForms\Program.cs)
- Application entry point
- Enables DPI awareness via SetProcessDPIAware()
- Configures Windows Forms settings

### Key Architecture Patterns
- Custom window styling with manual border rendering
- Windows API P/Invoke patterns using CsWin32 code generation
- Event-driven UI updates via Timer
- Panel-based magnifier hosting

## Known Issues and Development Priorities

Current development priorities are tracked in TASKS.md:
- DPI awareness and border math issues at non-100% scaling
- Title bar positioning relative to share region
- Window position/size persistence
- .NET 9 migration (completed)

## CI/CD
Automated builds run on push/PR via GitHub Actions, producing:
- Portable executables (x64/x86 self-contained)
- MSIX installer packages (x64/x86) for modern deployment

## TASKS.md Updates
When completing tasks, update the TASKS.md file to reflect current status and remove completed items.