# Troubleshooting Guide

## Cursor Visibility in Screen Capture Tools

### OBS Studio
**Issue**: Cursor may not be visible when using OBS Game Capture mode and the UltraWideScreenShare window is not focused.

**Solution**:
- Use **Window Capture** mode instead of Game Capture mode in OBS
- Alternatively, keep the UltraWideScreenShare window focused during capture
- Ensure "Capture Cursor" is enabled in the OBS source settings

**Note**: This is OBS-specific behavior related to how Game Capture handles unfocused windows. The cursor is being captured correctly by the application.

### Microsoft Teams
The cursor is captured and displayed correctly in Microsoft Teams screen sharing without any additional configuration.

### Discord and Other Screen Sharing Tools
Most professional screen sharing applications (Teams, Zoom, etc.) will capture and display the cursor correctly without any special configuration.

## Technical Details
UltraWideScreenShare uses the Windows Magnification API which includes hardware cursor capture by default. The cursor visibility issue in OBS Game Capture is specific to OBS's implementation and not a limitation of our application.