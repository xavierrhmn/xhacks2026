# PerfReg Usage Guide

## Overview
PerfReg is a performance regression detection tool that helps you track and compare benchmark results over time. It measures runtime, memory usage, and garbage collection metrics.

## Quick Start

```bash
# Run a single benchmark
dotnet run --project PerfReg run ./MyApp.exe

# Run with multiple iterations for accurate results
dotnet run --project PerfReg run ./MyApp.exe --runs 5

# Run with warmup to handle JIT compilation
dotnet run --project PerfReg run ./MyApp.exe --runs 5 --warmup 2

# View history
dotnet run --project PerfReg history

# Compare last two runs
dotnet run --project PerfReg compare
```

## Commands

### `run` - Execute Benchmarks

Run a benchmark and store results. Automatically compares with previous run if available.

**Basic usage:**
```bash
dotnet run --project PerfReg run <binary> [args...]
```

**Options:**
- `--runs N` - Run the benchmark N times and calculate statistics (default: 1)
- `--warmup N` - Run N warmup iterations before measuring (default: 0)

**Examples:**
```bash
# Single run
dotnet run --project PerfReg run ./MyApp.exe

# Pass arguments to your application
dotnet run --project PerfReg run ./MyApp.exe input.txt --verbose

# 10 runs for statistical confidence
dotnet run --project PerfReg run ./MyApp.exe --runs 10

# 3 warmup runs + 5 measured runs
dotnet run --project PerfReg run ./MyApp.exe --runs 5 --warmup 3
```

**What you'll see:**
- Warmup progress (if enabled)
- Individual run times (for multiple runs)
- Mean runtime with standard deviation
- Min, median, max values
- Peak memory usage statistics
- GC collection counts
- Automatic comparison with previous run

### `compare` - Compare Results

Compare the last two benchmark runs for any tracked program.

```bash
dotnet run --project PerfReg compare
```

**Output:**
- Timestamps and commit hashes for both runs
- Percentage change for each metric
- Warning symbols (⚠️) for regressions >5%
- Direction indicators (↑ worse, ↓ better, → unchanged)

### `history` - View All Results

Display historical benchmark data for all programs.

```bash
dotnet run --project PerfReg history
```

**Shows:**
- Total number of runs per program
- Last 5 runs with timestamps
- Runtime for each run
- Number of iterations (if multiple runs)
- Git commit hash

### `clear` - Delete History

Remove all benchmark history files.

```bash
dotnet run --project PerfReg clear
```

⚠️ **Warning:** This permanently deletes all `.benchmark.json` files. Cannot be undone.

### `config` - Create Configuration

Generate a default `.perfreg.json` configuration file.

```bash
dotnet run --project PerfReg config
```

## Configuration File

Create a `.perfreg.json` file in your project directory to customize default behavior:

```json
{
  "DefaultRuns": 5,
  "DefaultWarmupRuns": 2,
  "FailOnRegression": false,
  "Thresholds": {
    "RuntimePercent": 5.0,
    "MemoryPercent": 5.0,
    "GcGen0Percent": 5.0,
    "GcGen1Percent": 5.0,
    "GcGen2Percent": 5.0
  }
}
```

**Configuration options:**

| Option | Type | Default | Description |
|--------|------|---------|-------------|
| `DefaultRuns` | int | 1 | Number of benchmark runs (can override with --runs) |
| `DefaultWarmupRuns` | int | 0 | Number of warmup runs (can override with --warmup) |
| `FailOnRegression` | bool | false | Return non-zero exit code when regression detected (for CI/CD) |
| `Thresholds.RuntimePercent` | double | 5.0 | Runtime regression threshold (%) |
| `Thresholds.MemoryPercent` | double | 5.0 | Memory regression threshold (%) |
| `Thresholds.GcGen0Percent` | double | 5.0 | GC Gen0 regression threshold (%) |
| `Thresholds.GcGen1Percent` | double | 5.0 | GC Gen1 regression threshold (%) |
| `Thresholds.GcGen2Percent` | double | 5.0 | GC Gen2 regression threshold (%) |

## Tracked Metrics

### Runtime
- **Measurement**: Total execution time in milliseconds
- **Statistics**: Mean, median, standard deviation, min, max
- **Lower is better**

### Peak Memory
- **Measurement**: Maximum working set size in megabytes
- **Statistics**: Mean, median, standard deviation, min, max
- **Lower is better**

### GC Collections
- **Measurement**: Number of garbage collections per generation (0, 1, 2)
- **Statistics**: Mean, median, min, max
- **Lower is better** (fewer collections = better performance)

### Git Commit Hash
- Automatically captured for each run
- Helps correlate performance changes with code changes
- Shown as short hash (8 characters)

## Understanding Statistics

When you run benchmarks multiple times (--runs > 1), PerfReg calculates:

### Mean (Average)
The primary value stored and used for comparisons. Represents typical performance.

### Median
The middle value when sorted. Less affected by outliers than mean.

### Standard Deviation (±)
Indicates consistency:
- **Low StdDev** (~1-5ms): Consistent, reliable results
- **High StdDev** (>20ms): Inconsistent, may need more runs or warmup

### Min/Max
Shows the range of observed values:
- Large range suggests high variance
- Helps identify outliers

**Example output:**
```
Runtime:     209.11ms (±34.82ms)
             [min: 182.00ms, median: 184.99ms, max: 248.09ms]
```

This shows:
- Average runtime is 209ms
- Results vary by ±35ms (relatively high variance)
- Half the runs were faster than 185ms
- Slowest run was 248ms (possible outlier)

## Best Practices

### How Many Runs?

| Scenario | Recommended Runs | Warmup |
|----------|-----------------|--------|
| Quick check | 1 run | 0 |
| Development | 3-5 runs | 1-2 |
| Reliable benchmarks | 10-20 runs | 2-3 |
| Publication | 30+ runs | 5+ |

### When to Use Warmup

**Always use warmup for:**
- .NET applications (JIT compilation)
- First-time file access
- Database connections
- Network operations

**Example:**
```bash
# .NET app with 3 warmup runs
dotnet run --project PerfReg run MyApp.exe --runs 10 --warmup 3
```

### Interpreting Results

**Regression detected (⚠️):**
- Performance degraded by >5%
- Investigate recent code changes
- Check git commit hash for correlation

**High standard deviation:**
- Add more runs (--runs 10 or higher)
- Add warmup runs (--warmup 2+)
- Close background applications
- Check for system load

**Inconsistent results:**
- System might be under load
- Antivirus scanning
- Background updates
- Consider running at off-peak hours

## CI/CD Integration

### GitHub Actions Example

```yaml
name: Performance Tests

on: [pull_request]

jobs:
  benchmark:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v2

      - name: Setup .NET
        uses: actions/setup-dotnet@v2

      - name: Build
        run: dotnet build

      - name: Run Benchmarks
        run: |
          cd Perf_Regression_Detector
          dotnet run --project PerfReg run ../MyApp/bin/Debug/net8.0/MyApp.dll --runs 5

      - name: Upload Results
        uses: actions/upload-artifact@v2
        with:
          name: benchmark-results
          path: Perf_Regression_Detector/*.benchmark.json
```

### Exit Codes

- `0` - Success
- `1` - Error (invalid arguments, failed to run, etc.)
- Future: Non-zero when `FailOnRegression: true` and regression detected

## Troubleshooting

### "Failed to start process"
- Check that the binary path is correct
- Ensure the binary has execute permissions
- Use relative or absolute paths

### "No benchmark history found"
- Run at least one benchmark first
- Check you're in the correct directory
- Verify `.benchmark.json` files exist

### High variance in results
- Increase number of runs (--runs 20+)
- Add warmup runs (--warmup 3+)
- Close other applications
- Run on dedicated hardware

### Memory measurements seem wrong
- Memory is peak working set, not actual allocated
- .NET apps may pre-allocate memory
- GC may not have run yet
- Consider multiple runs for average

## File Format

Results are stored in `<program-name>.benchmark.json`:

```json
{
  "ProgramName": "TestProgram",
  "Results": [
    {
      "CommitHash": "da616b2",
      "Timestamp": "2026-02-01T01:21:08.123Z",
      "RuntimeMs": 209.11,
      "PeakMemoryBytes": 21811200,
      "Gen0Collections": 0,
      "Gen1Collections": 0,
      "Gen2Collections": 0,
      "Statistics": {
        "TotalRuns": 5,
        "WarmupRuns": 2,
        "Runtime": {
          "Mean": 209.11,
          "Median": 184.99,
          "StdDev": 34.82,
          "Min": 182.00,
          "Max": 248.09
        },
        "Memory": {
          "Mean": 21811200,
          "Median": 21790464,
          "StdDev": 94208,
          "Min": 21721088,
          "Max": 21970944
        },
        "GarbageCollection": {
          "Gen0": { "Mean": 0, "Median": 0, "Min": 0, "Max": 0 },
          "Gen1": { "Mean": 0, "Median": 0, "Min": 0, "Max": 0 },
          "Gen2": { "Mean": 0, "Median": 0, "Min": 0, "Max": 0 }
        }
      }
    }
  ]
}
```

## Tips & Tricks

### Compare Against Baseline
```bash
# Run baseline on main branch
git checkout main
dotnet run --project PerfReg run MyApp.exe --runs 10

# Switch to feature branch
git checkout feature-xyz
dotnet run --project PerfReg run MyApp.exe --runs 10

# Comparison shows difference between branches
```

### Batch Benchmarking
```bash
# Test multiple scenarios
for version in 1 2 3 4; do
  dotnet run --project PerfReg run TestProgram.exe $version --runs 5
done
```

### Export for Analysis
```bash
# Copy results for spreadsheet analysis
cp *.benchmark.json ~/benchmark-archive/
```

### Track Over Time
- Commit `.benchmark.json` files to git (optional)
- Track performance trends across commits
- Useful for long-term performance monitoring

## Next Steps

Now that you're familiar with Sprint 1 features, check out the [ROADMAP.md](ROADMAP.md) for upcoming features:
- Sprint 2: CI/CD Integration (exit codes, baseline comparison, JSON export)
- Sprint 3: Terminal charts and trend analysis
- Sprint 4+: HTML reports, CPU profiling, web dashboard

## Support

- View all commands: `dotnet run --project PerfReg help`
- Version info: `dotnet run --project PerfReg version`
- Report issues: Check project README

---

**Pro Tip:** Start with `--runs 5 --warmup 2` for most benchmarks. Adjust based on variance in results.
