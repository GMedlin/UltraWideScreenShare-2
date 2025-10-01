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
- Main project: `UltraWideScreenShare.WinForms` (.NET 9 Windows Forms application)
- Solution file: `UltraWideScreenShare2.sln`

## Architecture Overview

This is a Windows desktop application for ultra-wide screen sharing that uses:

### Core Technologies
- .NET 9 with Windows Forms
- Microsoft.Windows.CsWin32 for Win32 API access
- Desktop Duplication API (DXGI) for screen capture
- Vortice.Direct3D11 and Vortice.DXGI for DirectX interop
- Per-monitor DPI v2 awareness
- Portable-only distribution (self-contained executables)

### Key Components

#### MainWindow.cs
- Main application window with custom yellow border rendering
- Handles DPI scaling and window positioning
- Manages magnifierPanel (actually hosts Direct3D swap chain, not a magnifier)
- Uses 60fps timer (16ms interval) for real-time updates
- Implements window position/size persistence via Settings
- Manages transparency when cursor enters capture region
- Coordinates with detached title bar window

#### DesktopDuplicationCaptureController.cs
- Uses Desktop Duplication API (IDXGIOutputDuplication) for screen capture
- Creates Direct3D11 device and swap chain for rendering
- Implements self-capture detection via yellow border color sampling
- Handles monitor enumeration and output duplication setup
- Manages frame acquisition, region extraction, and presentation
- Implements error recovery and duplication reinitialization

#### TitleBarWindow.cs
- Detached title bar window that floats above main window
- Implements Windows-standard title bar buttons (minimize, maximize/restore, close)
- Uses system metrics (SM_CYCAPTION, SM_CYSIZE) for proper DPI-aware sizing
- Handles drag-to-move via parent window coordination
- Automatically hides when parent is minimized

#### Program.cs
- Application entry point
- Configures HighDpiMode.PerMonitorV2 for proper DPI scaling
- Initializes Windows Forms with visual styles

### Key Architecture Patterns
- Custom borderless window with manual border rendering
- Windows API P/Invoke patterns using CsWin32 code generation
- Direct3D11/DXGI interop for hardware-accelerated screen capture
- Event-driven UI updates via Timer (60fps)
- Self-capture prevention via pixel color sampling
- Detached title bar pattern for frameless windows

## Current Status

The application is fully functional with Desktop Duplication API for screen capture. Key features:
- Real-time screen region capture and display
- Self-capture prevention (detects own yellow border)
- DPI-aware UI scaling across all monitors
- Window position/size persistence
- Detached title bar with standard Windows controls
- Compatible with Teams and OBS screen sharing

## CI/CD
Automated builds run on push/PR via GitHub Actions, producing:
- Portable self-contained executables (x64/x86)
- Published as artifacts and GitHub releases

## TASKS.md Updates
When completing tasks, update the TASKS.md file to reflect current status and remove completed items.