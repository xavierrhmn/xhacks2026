# PerfReg Test Suite

Simple test application and automated demo scripts for demonstrating PerfReg features.

## Test Application: DemoApp

A configurable demo application with four performance scenarios:

### Scenarios

1. **fast** - Lightweight computation (~50ms)
   ```bash
   dotnet run --project PerfReg run DemoApp/bin/Debug/net8.0/DemoApp.exe fast
   ```

2. **slow** - Heavy computation (~300ms)
   ```bash
   dotnet run --project PerfReg run DemoApp/bin/Debug/net8.0/DemoApp.exe slow
   ```

3. **memory** - Memory-intensive workload (50MB allocation)
   ```bash
   dotnet run --project PerfReg run DemoApp/bin/Debug/net8.0/DemoApp.exe memory
   ```

4. **variable** - Variable performance with high variance
   ```bash
   dotnet run --project PerfReg run DemoApp/bin/Debug/net8.0/DemoApp.exe variable --runs 20
   ```

## Automated Demo Scripts

### Windows (PowerShell)
```powershell
.\run-demo.ps1
```

### Linux/Mac (Bash)
```bash
./run-demo.sh
```

### What the Scripts Do

1. Clear previous demo data
2. Run basic benchmark with statistics
3. Build benchmark history (multiple runs)
4. Show comparison between runs
5. Set performance baseline
6. Display benchmark history
7. **Show trend analysis with beautiful charts**

Takes about 30 seconds to complete.

## Manual Testing

### Quick 2-Minute Demo
```bash
# Clean slate
dotnet run --project PerfReg clear

# Basic benchmark
dotnet run --project PerfReg run DemoApp/bin/Debug/net8.0/DemoApp.exe fast --runs 10

# Show statistics
dotnet run --project PerfReg history

# Create regression
dotnet run --project PerfReg run DemoApp/bin/Debug/net8.0/DemoApp.exe slow --runs 10

# Compare
dotnet run --project PerfReg compare

# Build more history
dotnet run --project PerfReg run DemoApp/bin/Debug/net8.0/DemoApp.exe fast
dotnet run --project PerfReg run DemoApp/bin/Debug/net8.0/DemoApp.exe slow
dotnet run --project PerfReg run DemoApp/bin/Debug/net8.0/DemoApp.exe fast

# Show impressive charts
dotnet run --project PerfReg trend DemoApp
```

### Testing Percentiles (Tail Latency)
```bash
# Variable scenario shows importance of P95/P99
dotnet run --project PerfReg run DemoApp/bin/Debug/net8.0/DemoApp.exe variable --runs 30
```

You'll see P99 significantly higher than median, demonstrating tail latency.

### Testing CI/CD Integration
```bash
# Set baseline
dotnet run --project PerfReg run DemoApp/bin/Debug/net8.0/DemoApp.exe fast --runs 5
dotnet run --project PerfReg baseline set

# Test regression detection
dotnet run --project PerfReg run DemoApp/bin/Debug/net8.0/DemoApp.exe slow --fail-on-regression
echo $?  # Should be 1 (failure)
```

## Building the Demo App

If you need to rebuild:
```bash
cd DemoApp
dotnet build
```

## Files in This Test Suite

- `DemoApp/` - Test application source
  - `Program.cs` - Configurable performance scenarios
  - `DemoApp.csproj` - Project file
- `run-demo.ps1` - Windows PowerShell demo script
- `run-demo.sh` - Linux/Mac bash demo script
- `TEST_SUITE_README.md` - This file

## Tips for Presentations

1. Run the automated script first to build history
2. Then manually run `trend` command to show charts
3. Use `variable` scenario with 20+ runs to demonstrate percentiles
4. Use `--fail-on-regression` to show CI/CD integration
5. Export to JSON to show tooling integration

## Resetting Test Data

To start fresh:
```bash
dotnet run --project PerfReg clear
dotnet run --project PerfReg baseline clear
```

This removes all benchmark history and baselines.
