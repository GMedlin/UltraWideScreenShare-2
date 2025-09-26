# UltraWideScreenShare-2 Task Priority List

## Current Sprint: Core Functionality Fixes

### 🔴 High Priority (Core Issues)

- [ ] **Issue #2: DPI-awareness + border math bug**
  - Status: Ready for Claude
  - Description: Content cropping at non-100% DPI scaling
  - Impact: Core functionality broken on scaled displays
  - Action: Comment `@claude` on issue #2

- [ ] **Issue #4: Move title bar above share region**
  - Status: Ready for Claude
  - Description: Title bar overlaps with shared content area
  - Impact: Professional appearance for screen sharing
  - Action: Comment `@claude` on issue #4

### 🟡 Medium Priority (Quality of Life)

- [ ] **Issue #3: Remember window position and size**
  - Status: Ready for Claude
  - Description: Save/restore window bounds between sessions
  - Impact: User convenience
  - Action: Comment `@claude` on issue #3

### 🟢 Low Priority (Infrastructure)

- [ ] **Issue #5: Modernize to .NET 8**
  - Status: Ready for Claude
  - Description: Update from .NET 6 to .NET 8 for better support
  - Impact: Future maintenance and security
  - Action: Comment `@claude` on issue #5

## Setup Completed ✅

- [x] Claude Code GitHub Action configured with OAuth token
- [x] CI/CD build workflow for automatic builds
- [x] .gitignore updated for Claude Code local settings
- [x] Issues created with detailed implementation plans

## Development Workflow

1. **Test Changes**: Comment `@claude` on GitHub issues
2. **Local Build**: `dotnet build UltraWideScreenShare.WinForms`
3. **Local Run**: `dotnet run --project UltraWideScreenShare.WinForms` (requires .NET 6 runtime)
4. **CI/CD**: Automatic builds on push/PR create downloadable artifacts

## Notes

- Start with Issue #2 (DPI bug) as it affects core functionality
- Install .NET 6 runtime locally for testing: https://dotnet.microsoft.com/en-us/download/dotnet/6.0
- Modernization (Issue #5) should be done last to avoid complications
- All issues have detailed implementation guidance for Claude