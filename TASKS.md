# UltraWideScreenShare-2 Task Priority List

## 🚀 New Priority: Cursor Visibility in Screen Captures

### Problem Description
Mouse cursor is not visible in OBS captures or screen sharing tools when other windows are focused. This is a core limitation of Desktop Duplication API.

### Issues Experienced
1. **Desktop Duplication API Limitation**: API never includes cursor in captured frames - cursor is hardware overlay
2. **Complex Manual Compositing**: Attempted cursor rendering resulted in:
   - Odd-looking boxes instead of proper cursor
   - Pink triangles when using alpha blending
   - Complex data format interpretation (BGRA, pitch, hotspot calculations)
   - Performance overhead with CPU-based pixel manipulation

### Approaches Tried
1. **Manual Cursor Compositing** ❌
   - Captured cursor data from `GetFramePointerShape`
   - Attempted CPU-based alpha blending
   - Result: Visual artifacts, incorrect rendering

2. **Windows Graphics Capture API Migration** ❌ (Incomplete)
   - Modern API with `IsCursorCaptureEnabled = true` property
   - Result: Complex migration, build errors, interop complexity

### Recommended Next Approach
**Use a proven screen capture library that handles cursor automatically:**

- **ScreenCapture.NET**: Modern .NET library with built-in cursor support
- **FFMpegCore with screen capture**: Handles cursor compositing natively
- **Windows.Graphics.Capture wrapper libraries**: Pre-built solutions for cursor handling

### Future Implementation Prompt
```
"I need to add mouse cursor visibility to screen captures in my .NET 9 Windows Forms application. Currently using Desktop Duplication API but cursor isn't visible in OBS/screen sharing tools.

Requirements:
- Show cursor in all capture scenarios (OBS, Teams, etc.)
- Maintain current crisp display quality (using DXGI Scaling.None)
- Keep existing architecture if possible
- Must work when other windows are focused

Current setup:
- .NET 9 Windows Forms app
- Desktop Duplication API with DirectX 11
- Vortice.Direct3D11 and Vortice.DXGI packages
- Custom transparency handling with TransparencyKey

Please recommend and implement the simplest, most reliable solution - whether that's using a proven library, migrating to Windows Graphics Capture API with proper interop, or another approach."
```

## Development Workflow

1. **Local Build**: `dotnet build UltraWideScreenShare.WinForms`
2. **Local Run**: `dotnet run --project UltraWideScreenShare.WinForms` (requires .NET 9 runtime)
3. **CI/CD**: Automatic builds on push/PR create downloadable artifacts