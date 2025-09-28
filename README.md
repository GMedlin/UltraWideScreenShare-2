# UltraWideScreenShare 2

A Windows desktop application for ultra-wide screen sharing that allows precise control over which portion of your screen to share.

## Features

- **Precise Screen Region Selection**: Choose exactly which part of your ultra-wide monitor to share
- **Real-time Preview**: See exactly what you're sharing with the built-in magnifier
- **DPI Aware**: Works correctly across different display scaling settings
- **Lightweight**: No installation required - just extract and run

## Installation

UltraWideScreenShare 2 is distributed as a portable application:

1. **Download** the latest release from the [Releases page](https://github.com/GMedlin/UltraWideScreenShare-2/releases)
2. **Choose your architecture**:
   - `UltraWideScreenShare2-win-x64.zip` for 64-bit Windows
   - `UltraWideScreenShare2-win-x86.zip` for 32-bit Windows
3. **Extract** the zip file to your desired location (e.g., `C:\Apps\UltraWideScreenShare2\`)
4. **Run** `UltraWideScreenShare2.exe`

No administrative privileges or Windows Store installation required!

## System Requirements

- Windows 10 or later
- .NET 9 runtime (included in the portable distribution)
- Multi-monitor setup recommended for best experience

## Usage

1. Launch the application
2. Position and resize the application window to frame the area you want to share
3. The magnifier panel shows exactly what will be captured
4. Use this window for screen sharing in your video conferencing application

## Architecture

- **Framework**: .NET 9 with Windows Forms
- **Screen Capture**: Windows Magnification API
- **DPI Support**: Automatic scaling detection and handling
- **Performance**: 30fps real-time updates

## Building from Source

```bash
# Clone the repository
git clone https://github.com/GMedlin/UltraWideScreenShare-2.git
cd UltraWideScreenShare-2

# Build for your platform
dotnet build UltraWideScreenShare.WinForms --configuration Release

# Or create a portable executable
dotnet publish UltraWideScreenShare.WinForms -c Release -r win-x64 --self-contained -o ./publish/x64
```

## License

[Add your license information here]

## Contributing

Issues and pull requests are welcome! Please check the [TASKS.md](TASKS.md) file for current development priorities.