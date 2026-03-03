# Fix for Issue #169: QualificationRunner Cannot Start from R

## Summary

This document describes the fix applied to resolve the `System.IO.FileLoadException` that prevented QualificationRunner from starting when invoked from R or RStudio.

## Problem

When launching QualificationRunner from R using `system()`:
```r
system('"C:\\Qualification-Runner.12.2.232\\QualificationRunner.exe" --help')
```

The application would fail with:
```
System.IO.FileLoadException: Could not load file or assembly "Microsoft.Extensions.Logging.Abstractions, Version=9.0.0.0"...
```

However, running the same command from Windows CMD worked correctly.

## Root Cause

The issue was caused by version conflicts in the `Microsoft.Extensions.*` packages:

1. QualificationRunner explicitly references `Microsoft.Extensions.Logging.Console` v3.1.0
2. The OSPSuite packages (v12.3.1) reference newer versions of `Microsoft.Extensions.Logging.Abstractions` (v9.0.0.0)
3. Without assembly binding redirects, the .NET Framework doesn't know which version to use
4. R's execution context made this issue more apparent than CMD

## Solution Applied

Two complementary approaches were implemented:

### 1. Added Assembly Binding Redirects

Updated `src/QualificationRunner/App.config` to include explicit binding redirects for all `Microsoft.Extensions.*` packages. These redirects tell the .NET runtime to use version 9.0.0.0 regardless of which version is requested.

The redirects cover:
- Microsoft.Extensions.Logging.Abstractions
- Microsoft.Extensions.Logging
- Microsoft.Extensions.DependencyInjection.Abstractions
- Microsoft.Extensions.Options
- Microsoft.Extensions.Primitives
- Microsoft.Extensions.Configuration.Abstractions
- Microsoft.Extensions.DependencyInjection

### 2. Enabled AutoGenerateBindingRedirects

Added the following properties to `src/QualificationRunner/QualificationRunner.csproj`:
```xml
<AutoGenerateBindingRedirects>true</AutoGenerateBindingRedirects>
<GenerateBindingRedirectsOutputType>true</GenerateBindingRedirectsOutputType>
```

This ensures that binding redirects are automatically updated during build if dependency versions change.

## Testing the Fix

After rebuilding the application, users should verify:

### From Windows CMD:
```cmd
"C:\path\to\QualificationRunner.exe" --help
```

### From R/RStudio:
```r
system('"C:\\path\\to\\QualificationRunner.exe" --help')
```

### From PowerShell:
```powershell
& "C:\path\to\QualificationRunner.exe" --help
```

All three methods should now work correctly without assembly loading errors.

## Important Notes

### Version Numbers in Binding Redirects

The binding redirects currently point to version 9.0.0.0. If OSPSuite packages are updated to reference different versions of Microsoft.Extensions packages, the version numbers in App.config may need to be adjusted.

To determine the correct version:
1. Build the project successfully
2. Check the bin folder: `src/QualificationRunner/bin/Debug/net472/` or `bin/Release/net472/`
3. Look for `Microsoft.Extensions.*.dll` files
4. Check their properties/version information
5. Update the `newVersion` attributes in App.config to match the highest version found

### AutoGenerateBindingRedirects

The `AutoGenerateBindingRedirects` feature should automatically update the App.config during build. However, if issues persist:

1. Clean the solution: `dotnet clean`
2. Delete bin and obj folders
3. Rebuild: `dotnet build`
4. Verify the App.config has been updated with correct versions

### If the Issue Persists

If QualificationRunner still fails to start from R after this fix:

1. **Check the actual error message** - it may be a different assembly now
2. **Verify the App.config is deployed** - ensure it's in the same folder as QualificationRunner.exe
3. **Check assembly versions** - run this from the QualificationRunner folder:
   ```powershell
   Get-ChildItem *.dll | ForEach-Object { [System.Reflection.AssemblyName]::GetAssemblyName($_.FullName) }
   ```
4. **Enable Fusion logging** to see detailed assembly binding information:
   - Use the Assembly Binding Log Viewer (fuslogvw.exe)
   - Or add registry keys to enable fusion logging

5. **Report findings** on the GitHub issue with:
   - The new error message
   - The output of checking assembly versions
   - Fusion logs if available

## Alternative Workarounds (If Fix Doesn't Work)

If the assembly binding redirects don't fully resolve the issue:

### Workaround 1: Set Working Directory in R

```r
# Save current directory
original_dir <- getwd()

# Change to QualificationRunner directory
setwd("C:\\path\\to\\QualificationRunner")

# Run the application
system('"QualificationRunner.exe" --help')

# Restore original directory
setwd(original_dir)
```

### Workaround 2: Use Full Paths and Shell Execute

```r
# Use shell with full paths
shell('"C:\\path\\to\\QualificationRunner.exe" --help')
```

### Workaround 3: Create a Batch File Wrapper

Create `RunQualificationRunner.bat`:
```batch
@echo off
cd /d "%~dp0"
QualificationRunner.exe %*
```

Then call from R:
```r
system('"C:\\path\\to\\RunQualificationRunner.bat" --help')
```

## Related Links

- Issue: https://github.com/Open-Systems-Pharmacology/QualificationRunner/issues/169
- Microsoft Docs: [Redirecting Assembly Versions](https://docs.microsoft.com/en-us/dotnet/framework/configure-apps/redirect-assembly-versions)
- Assembly Binding Log Viewer: [fuslogvw.exe](https://docs.microsoft.com/en-us/dotnet/framework/tools/fuslogvw-exe-assembly-binding-log-viewer)

## Changes Made

### Files Modified:
1. `src/QualificationRunner/App.config` - Added assembly binding redirects
2. `src/QualificationRunner/QualificationRunner.csproj` - Enabled AutoGenerateBindingRedirects

### Files Created:
1. `ISSUE_169_ANALYSIS.md` - Detailed analysis of the problem
2. `ISSUE_169_FIX.md` - This document describing the fix and testing procedures
