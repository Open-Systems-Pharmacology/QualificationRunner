# Issue #169 Analysis: Assembly Loading Error When Starting from R

## Problem Summary

QualificationRunner versions 12.1 and 12.2 fail to start when invoked from R/RStudio using `system()`, but work correctly when launched from Windows CMD. The error is:

```
System.IO.FileLoadException: Could not load file or assembly "Microsoft.Extensions.Logging.Abstractions, Version=9.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60" or one of its dependencies. The located assembly's manifest definition does not match the assembly reference.
```

## Root Cause Analysis

### 1. Version Conflict in Logging Dependencies

The application has conflicting version requirements for `Microsoft.Extensions.Logging.Abstractions`:

- **QualificationRunner projects** reference `Microsoft.Extensions.Logging.Console` v3.1.0
  - This package transitively depends on `Microsoft.Extensions.Logging.Abstractions` v3.1.x

- **OSPSuite packages** (v12.3.1) appear to depend on newer versions
  - The error shows version 9.0.0.0 is being requested
  - This is a much newer version from .NET 9.0 era

### 2. Missing Assembly Binding Redirects

The `App.config` file contains NO assembly binding redirects:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
    <startup>
        <supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.7.2"/>
    </startup>
</configuration>
```

For .NET Framework applications, when multiple versions of the same assembly are referenced (directly or transitively), assembly binding redirects are required to tell the runtime which version to use.

### 3. Why It Works from CMD but Not from R

The behavior difference between CMD and R execution suggests:

1. **Assembly Probing Paths**: R may alter the working directory or assembly probing paths, affecting how the CLR searches for assemblies
2. **Environment Variables**: R's environment may have different PATH or other variables that influence assembly resolution
3. **Process Context**: R's process spawning mechanism may create a different execution context than CMD
4. **Timing**: The assembly resolution happens at runtime, and R's context makes the version mismatch more evident

When run from CMD, the .NET Framework might be finding and using a compatible version through assembly probing, but R's execution context prevents this fallback mechanism from working.

## Proposed Solutions

### Solution 1: Add Assembly Binding Redirects (RECOMMENDED)

**Approach**: Add explicit assembly binding redirects to `App.config` to resolve the version conflicts.

**Implementation**:

Update `/src/QualificationRunner/App.config` to include:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
    <startup>
        <supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.7.2"/>
    </startup>
    <runtime>
        <assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1">
            <!-- Redirect all versions of Microsoft.Extensions.Logging.Abstractions to the version that's actually deployed -->
            <dependentAssembly>
                <assemblyIdentity name="Microsoft.Extensions.Logging.Abstractions"
                                  publicKeyToken="adb9793829ddae60"
                                  culture="neutral" />
                <bindingRedirect oldVersion="0.0.0.0-9.0.0.0" newVersion="9.0.0.0" />
            </dependentAssembly>

            <!-- Additional redirects that may be needed for other Microsoft.Extensions.* packages -->
            <dependentAssembly>
                <assemblyIdentity name="Microsoft.Extensions.Logging"
                                  publicKeyToken="adb9793829ddae60"
                                  culture="neutral" />
                <bindingRedirect oldVersion="0.0.0.0-9.0.0.0" newVersion="9.0.0.0" />
            </dependentAssembly>

            <dependentAssembly>
                <assemblyIdentity name="Microsoft.Extensions.DependencyInjection.Abstractions"
                                  publicKeyToken="adb9793829ddae60"
                                  culture="neutral" />
                <bindingRedirect oldVersion="0.0.0.0-9.0.0.0" newVersion="9.0.0.0" />
            </dependentAssembly>

            <dependentAssembly>
                <assemblyIdentity name="Microsoft.Extensions.Options"
                                  publicKeyToken="adb9793829ddae60"
                                  culture="neutral" />
                <bindingRedirect oldVersion="0.0.0.0-9.0.0.0" newVersion="9.0.0.0" />
            </dependentAssembly>

            <dependentAssembly>
                <assemblyIdentity name="Microsoft.Extensions.Primitives"
                                  publicKeyToken="adb9793829ddae60"
                                  culture="neutral" />
                <bindingRedirect oldVersion="0.0.0.0-9.0.0.0" newVersion="9.0.0.0" />
            </dependentAssembly>
        </assemblyBinding>
    </runtime>
</configuration>
```

**Note**: The exact version numbers (9.0.0.0 vs others) need to be determined by examining the actual bin folder after a successful build. Use the highest version present.

**Pros**:
- Minimal code changes
- Standard .NET Framework solution for version conflicts
- Should work consistently across different execution contexts (CMD, R, PowerShell, etc.)

**Cons**:
- Requires knowing exact version numbers deployed
- Need to verify the target version actually exists in the deployment

### Solution 2: Auto-Generate Binding Redirects

**Approach**: Enable automatic binding redirect generation in the project file.

**Implementation**:

Update both `.csproj` files to include:

```xml
<PropertyGroup>
    <AutoGenerateBindingRedirects>true</AutoGenerateBindingRedirects>
    <GenerateBindingRedirectsOutputType>true</GenerateBindingRedirectsOutputType>
</PropertyGroup>
```

**Pros**:
- Automatic management of binding redirects
- Adapts to dependency changes automatically

**Cons**:
- May generate more redirects than necessary
- Build process must support this feature
- Needs to be verified in the build pipeline

### Solution 3: Align Package Versions

**Approach**: Update all `Microsoft.Extensions.Logging.*` packages to use the same version across all dependencies.

**Implementation**:

Explicitly reference the same version of logging packages that OSPSuite.* packages use:

```xml
<PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="9.0.0" />
<PackageReference Include="Microsoft.Extensions.Logging" Version="9.0.0" />
<PackageReference Include="Microsoft.Extensions.Logging.Console" Version="9.0.0" />
```

**Pros**:
- Eliminates version conflicts at the source
- More maintainable long-term

**Cons**:
- Requires updating to newer packages (compatibility risk)
- May require code changes if APIs have changed
- Larger dependency footprint

### Solution 4: Create App.config.template for Build Process

**Approach**: Generate binding redirects during the build process based on actual resolved assemblies.

**Implementation**:

Add a post-build step or Rake task that:
1. Analyzes the bin folder for all Microsoft.Extensions.* assemblies
2. Generates appropriate binding redirects
3. Updates App.config with the correct versions

**Pros**:
- Always accurate to the actual build output
- Adapts to version changes automatically

**Cons**:
- More complex build process
- Requires custom tooling
- More difficult to troubleshoot

## Recommended Action Plan

**Immediate Fix (Short-term)**:
1. Implement **Solution 1** - Add explicit assembly binding redirects
2. Determine exact version numbers by examining a successful build's bin folder
3. Test from both CMD and R to verify the fix

**Long-term Improvement**:
1. Implement **Solution 2** - Enable AutoGenerateBindingRedirects
2. Consider **Solution 3** - Align all package versions when OSPSuite packages are updated
3. Add integration tests that verify the application starts correctly from different execution contexts

## Testing Verification

After implementing the fix, verify:

1. **From CMD**: `QualificationRunner.exe --help` (should work as before)
2. **From R**: `system('"path\to\QualificationRunner.exe" --help')` (should now work)
3. **From PowerShell**: `& "path\to\QualificationRunner.exe" --help` (should work)
4. **Full qualification run**: Verify actual qualification tasks still execute correctly

## Additional Notes

- The issue appeared after upgrading to OSPSuite v12.3.x packages
- Version 12.0 worked because it likely had compatible versions or different dependencies
- This is a common issue in .NET Framework when mixing packages from different eras
- The root cause is not specific to R, but R's execution context exposes the issue more readily

## References

- Issue: https://github.com/Open-Systems-Pharmacology/QualificationRunner/issues/169
- Microsoft Docs: [Redirecting Assembly Versions](https://docs.microsoft.com/en-us/dotnet/framework/configure-apps/redirect-assembly-versions)
- Stack Overflow: Similar FileLoadException issues with Microsoft.Extensions packages
