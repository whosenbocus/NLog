# .NET 9.0 Upgrade Plan

## Execution Steps

1. Validate that an .NET 9.0 SDK required for this upgrade is installed on the machine and if not, help to get it installed.
2. Ensure that the SDK version specified in global.json files is compatible with the .NET 9.0 upgrade.
3. Upgrade projects to .NET 9.0.
  - 3.1. Upgrade src/NLog/NLog.csproj
  - 3.2. Upgrade src/NLog.Targets.AtomicFile/NLog.Targets.AtomicFile.csproj
  - 3.3. Upgrade src/NLog.Targets.ConcurrentFile/NLog.Targets.ConcurrentFile.csproj
  - 3.4. Upgrade benchmarks/NLog.Benchmarks/NLog.Benchmarks.csproj
  - 3.5. Upgrade tests/NLog.Targets.AtomicFile.Tests/NLog.Targets.AtomicFile.Tests.csproj
  - 3.6. Upgrade tests/NLog.Targets.ConcurrentFile.Tests/NLog.Targets.ConcurrentFile.Tests.csproj
  - 3.7. Upgrade tests/TestTrimPublish/TestTrimPublish.csproj
  - 3.8. Upgrade src/NLog.OutputDebugString/NLog.OutputDebugString.csproj
  - 3.9. Upgrade tests/NLog.UnitTests/NLog.UnitTests.csproj
4. Run unit tests to validate upgrade in the projects listed below:
  - tests/NLog.Targets.AtomicFile.Tests/NLog.Targets.AtomicFile.Tests.csproj
  - tests/NLog.Targets.ConcurrentFile.Tests/NLog.Targets.ConcurrentFile.Tests.csproj
  - tests/NLog.UnitTests/NLog.UnitTests.csproj

## Settings

This section contains settings and data used by execution steps.

### Excluded projects

| Project name | Description |
|:-----------------------------------------------|:---------------------------:|

### Aggregate NuGet packages modifications across all projects

| Package Name | Current Version | New Version | Description |
|:------------------------------------|:---------------:|:-----------:|:------------------------------------|
| DotNetZip.Reduced | 1.9.1.8 | None | No supported version found for .NET 9.0 |
| System.ValueTuple | 4.5.0 | None | Functionality included with new framework reference |

### Project upgrade details

#### src/NLog/NLog.csproj modifications
Project properties changes:
  - Target frameworks should be changed from `net35;net46;netstandard2.0;netstandard2.1` to `net9.0`

#### src/NLog.Targets.AtomicFile/NLog.Targets.AtomicFile.csproj modifications
Project properties changes:
  - Target frameworks should be changed from `net35;net46;net8.0` to `net9.0`

#### src/NLog.Targets.ConcurrentFile/NLog.Targets.ConcurrentFile.csproj modifications
Project properties changes:
  - Target frameworks should be changed from `net35;net46;netstandard2.0` to `net9.0`

#### benchmarks/NLog.Benchmarks/NLog.Benchmarks.csproj modifications
Project properties changes:
  - Target frameworks should be changed from `net462;net6.0;net8.0` to `net9.0`

#### tests/NLog.Targets.AtomicFile.Tests/NLog.Targets.AtomicFile.Tests.csproj modifications
Project properties changes:
  - Target frameworks should be changed from `net462;net8.0` to `net9.0`

#### tests/NLog.Targets.ConcurrentFile.Tests/NLog.Targets.ConcurrentFile.Tests.csproj modifications
Project properties changes:
  - Target frameworks should be changed from `net462;net6.0` to `net9.0`
NuGet packages changes:
  - DotNetZip.Reduced should be removed (no supported version for .NET 9.0)

#### tests/TestTrimPublish/TestTrimPublish.csproj modifications
Project properties changes:
  - Target frameworks should be changed from `net8.0` to `net9.0`

#### src/NLog.OutputDebugString/NLog.OutputDebugString.csproj modifications
Project properties changes:
  - Target frameworks should be changed from `net35;net46;netstandard2.0;netstandard2.1` to `net9.0`

#### tests/NLog.UnitTests/NLog.UnitTests.csproj modifications
Project properties changes:
  - Target frameworks should be changed from `net462;net6.0` to `net9.0`
NuGet packages changes:
  - DotNetZip.Reduced should be removed (no supported version for .NET 9.0)
  - System.ValueTuple should be removed (functionality included with new framework reference)
