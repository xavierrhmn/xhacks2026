# PerfReg Usage Guide

## Overview
PerfReg is a comprehensive performance regression detection tool that helps you track, analyze, and visualize benchmark results over time. It measures runtime, memory usage, and garbage collection metrics with professional-grade statistical analysis and beautiful terminal visualization.

## Quick Start

```bash
# Run a single benchmark
dotnet run --project PerfReg run ./MyApp.exe

# Run with multiple iterations for accurate results
dotnet run --project PerfReg run ./MyApp.exe --runs 10

# Run with warmup to handle JIT compilation
dotnet run --project PerfReg run ./MyApp.exe --runs 10 --warmup 3

# View performance trends with charts
dotnet run --project PerfReg trend

# Set baseline and fail CI on regression
dotnet run --project PerfReg baseline set
dotnet run --project PerfReg run ./MyApp.exe --fail-on-regression
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
- `--fail-on-regression` - Exit with code 1 if regression detected (default: false)

**Examples:**
```bash
# Single run
dotnet run --project PerfReg run ./MyApp.exe

# Pass arguments to your application
dotnet run --project PerfReg run ./MyApp.exe input.txt --verbose

# 10 runs for statistical confidence with percentiles
dotnet run --project PerfReg run ./MyApp.exe --runs 10

# 3 warmup runs + 5 measured runs
dotnet run --project PerfReg run ./MyApp.exe --runs 5 --warmup 3

# Fail CI build on regression
dotnet run --project PerfReg run ./MyApp.exe --runs 5 --fail-on-regression
```

**What you'll see:**
- Warmup progress (if enabled)
- Individual run times (for multiple runs)
- Mean runtime with standard deviation
- Min, median, max values
- **P50, P95, P99 percentiles** (Sprint 3)
- Peak memory usage statistics
- GC collection counts
- Automatic comparison with previous run
- Exit code 1 if regression detected (with --fail-on-regression)

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

### `trend` - Show Performance Trends (Sprint 3)

Visualize performance trends with beautiful terminal charts.

```bash
dotnet run --project PerfReg trend [program] [--window N]
```

**Options:**
- `program` - Specific program to analyze (default: first program)
- `--window N` - Number of recent runs to analyze (default: 10)

**What you'll see:**
- Overall trend direction (Improving/Stable/Degrading)
- Per-metric trend analysis with linear regression
- **ASCII line charts** showing performance over time
- **Sparklines** for quick visual overview
- **Distribution histogram** for multiple runs
- Color-coded visualization (green/yellow/red)

**Example output:**
```
╔════════════════════════════════════════════════════════════════╗
║                    Trend Analysis: MyApp                      ║
╚════════════════════════════════════════════════════════════════╝

Overall Trend: ✓ Improving
Analysis Window: Last 10 runs

Metric Trends:
  ✓ Runtime        :      -5.2% over 10 runs
  → Memory         :      +0.8% over 10 runs

Runtime Trend (last 10 runs):

  250.45 ┤
       │●─
       │  ──
       │    ──●
       │       ───●
       │          ──●
       │             ──●
       │                ───●
       │                   ──●
       │                      ──●
       │                         ●
  200.12 └────────────────────────────────────────────────────────────
       First                                                  Last

Quick View:
  Runtime:  █▇▆▅▄▃▃▂▂▁  250ms → 200ms
  Memory:   ▃▃▃▄▄▃▃▃▃▃  45.2MB → 45.6MB
```

### `compare-historical` - Historical Comparison (Sprint 3)

Compare current performance against any previous commit.

```bash
dotnet run --project PerfReg compare-historical <commit-hash> [program]
```

**Parameters:**
- `commit-hash` - Short or full commit hash to compare against
- `program` - Optional program name (default: first program)

**Example:**
```bash
dotnet run --project PerfReg compare-historical abc1234
dotnet run --project PerfReg compare-historical abc1234 MyApp
```

**What you'll see:**
- Comparison between current and target commit
- Trend over all commits between them
- Sparkline showing progression
- Percentage changes for all metrics

### `baseline` - Manage Performance Baselines (Sprint 2)

Set and compare against stable performance baselines.

**Subcommands:**
- `baseline set [program]` - Mark current run as baseline
- `baseline compare [program]` - Compare latest run against baseline
- `baseline show [program]` - Display current baselines
- `baseline clear <program>` - Remove baseline

**Examples:**
```bash
# Set baseline on main branch
git checkout main
dotnet run --project PerfReg run ./MyApp.exe --runs 10
dotnet run --project PerfReg baseline set

# On feature branch, compare against baseline
git checkout feature/optimization
dotnet run --project PerfReg run ./MyApp.exe --runs 10
dotnet run --project PerfReg baseline compare

# Show all baselines
dotnet run --project PerfReg baseline show

# Clear baseline
dotnet run --project PerfReg baseline clear MyApp
```

**Use cases:**
- Compare feature branches against main
- Track performance across releases
- Validate optimizations against stable reference

### `export` - Export as JSON (Sprint 2)

Export benchmark data in structured JSON format for tooling integration.

```bash
dotnet run --project PerfReg export [program]
```

**Examples:**
```bash
# Export all programs
dotnet run --project PerfReg export > results.json

# Export specific program
dotnet run --project PerfReg export MyApp > myapp-results.json

# Pipe to jq for analysis
dotnet run --project PerfReg export | jq '.Results[-1].RuntimeMs'

# Send to monitoring system
dotnet run --project PerfReg export | curl -X POST http://metrics.example.com/api/benchmarks -d @-
```

**Output format:**
```json
[
  {
    "ProgramName": "MyApp",
    "Results": [
      {
        "CommitHash": "abc1234",
        "Timestamp": "2026-02-01T12:00:00Z",
        "RuntimeMs": 245.67,
        "PeakMemoryBytes": 52428800,
        "Gen0Collections": 5,
        "Gen1Collections": 2,
        "Gen2Collections": 0,
        "Statistics": {
          "TotalRuns": 10,
          "WarmupRuns": 2,
          "Runtime": {
            "Mean": 245.67,
            "Median": 243.21,
            "StdDev": 12.34,
            "Min": 230.45,
            "Max": 265.89,
            "P50": 243.21,
            "P95": 261.45,
            "P99": 264.23
          }
        }
      }
    ]
  }
]
```

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
| `FailOnRegression` | bool | false | Return non-zero exit code when regression detected |
| `Thresholds.RuntimePercent` | double | 5.0 | Runtime regression threshold (%) |
| `Thresholds.MemoryPercent` | double | 5.0 | Memory regression threshold (%) |
| `Thresholds.GcGen0Percent` | double | 5.0 | GC Gen0 regression threshold (%) |
| `Thresholds.GcGen1Percent` | double | 5.0 | GC Gen1 regression threshold (%) |
| `Thresholds.GcGen2Percent` | double | 5.0 | GC Gen2 regression threshold (%) |

## Tracked Metrics

### Runtime
- **Measurement**: Total execution time in milliseconds
- **Statistics**: Mean, median, standard deviation, min, max, P50, P95, P99
- **Lower is better**

### Peak Memory
- **Measurement**: Maximum working set size in megabytes
- **Statistics**: Mean, median, standard deviation, min, max, P50, P95, P99
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

### Mean (Average)
The primary value stored and used for comparisons. Represents typical performance.

### Median
The middle value when sorted. Less affected by outliers than mean.

### Standard Deviation (±)
Indicates consistency:
- **Low StdDev** (~1-5ms): Consistent, reliable results
- **High StdDev** (>20ms): Inconsistent, may need more runs or warmup

### Min/Max
Shows the range of observed values. Large range suggests high variance.

### Percentiles (P50/P95/P99) - Sprint 3
**Critical for understanding tail latency:**

- **P50 (50th percentile)**: Same as median - half the runs are faster
- **P95 (95th percentile)**: 95% of runs are faster than this value
- **P99 (99th percentile)**: 99% of runs are faster than this value

**Why percentiles matter:**
- Mean hides outliers
- P95/P99 show worst-case user experience
- Critical for production SLAs
- Industry standard for performance monitoring

**Example interpretation:**
```
Runtime:     245.67ms (±12.34ms)
             [min: 230.45ms, median: 243.21ms, max: 265.89ms]
             [p50: 243.21ms, p95: 261.45ms, p99: 264.23ms]
```

This shows:
- Average runtime is 246ms
- Most users see ~243ms (P50)
- 5% of requests take >261ms (P95) ⚠️ Tail latency concern
- 1% of requests take >264ms (P99) ⚠️ Need investigation

## Best Practices

### How Many Runs?

| Scenario | Recommended Runs | Warmup |
|----------|-----------------|--------|
| Quick check | 1 run | 0 |
| Development | 5-10 runs | 2-3 |
| Reliable benchmarks | 20-30 runs | 3-5 |
| Production/Release | 50+ runs | 5-10 |
| Percentile accuracy | 100+ runs | 10 |

### When to Use Warmup

**Always use warmup for:**
- .NET applications (JIT compilation)
- First-time file access
- Database connections
- Network operations
- Cached operations

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
- Add more runs (--runs 20 or higher)
- Add warmup runs (--warmup 3+)
- Close background applications
- Check for system load

**High P99 but good mean:**
- Tail latency issue
- Indicates occasional slowdowns
- Could be GC pauses, cache misses, or contention
- View histogram to see distribution

**Inconsistent results:**
- System might be under load
- Antivirus scanning
- Background updates
- Consider running at off-peak hours

## Visual Analysis (Sprint 3)

### Line Charts
Show performance trends over time with full ASCII art:
- 60×10 character grid
- Labeled axes with min/max values
- Connected data points
- Color-coded (cyan for data, dark cyan for lines)

**Best for:** Understanding long-term trends

### Sparklines
Compact one-line visualizations:
- Uses characters ▁▂▃▄▅▆▇█ for height levels
- Color-coded by performance (green=good, red=bad)
- Perfect for quick overview

**Best for:** Quick status at a glance

### Histograms
Show distribution of results:
- 10 buckets across value range
- Bar width shows frequency
- Reveals bimodal distributions or outliers

**Best for:** Understanding result distribution

## CI/CD Integration

### GitHub Actions Example

```yaml
name: Performance Tests

on: [pull_request, push]

jobs:
  benchmark:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v2
        with:
          fetch-depth: 0  # Need history for comparison

      - name: Setup .NET
        uses: actions/setup-dotnet@v2
        with:
          dotnet-version: '8.0.x'

      - name: Build
        run: dotnet build -c Release

      - name: Run Benchmarks
        run: |
          cd Perf_Regression_Detector
          dotnet run --project PerfReg run ../MyApp/bin/Release/net8.0/MyApp.dll \
            --runs 10 \
            --warmup 3 \
            --fail-on-regression

      - name: Upload Results
        if: always()
        uses: actions/upload-artifact@v2
        with:
          name: benchmark-results
          path: Perf_Regression_Detector/*.benchmark.json

      - name: Export JSON
        if: always()
        run: |
          cd Perf_Regression_Detector
          dotnet run --project PerfReg export > $GITHUB_WORKSPACE/benchmark-results.json

      - name: Upload JSON
        if: always()
        uses: actions/upload-artifact@v2
        with:
          name: benchmark-json
          path: benchmark-results.json
```

### Setting Baselines in CI

```yaml
      - name: Set Baseline (main branch only)
        if: github.ref == 'refs/heads/main'
        run: |
          cd Perf_Regression_Detector
          dotnet run --project PerfReg baseline set
```

### GitLab CI
```yaml
benchmark:
  stage: test
  script:
    - cd Perf_Regression_Detector
    - dotnet run --project PerfReg run ../MyApp/bin/Release/net8.0/MyApp.dll --runs 10 --fail-on-regression
  artifacts:
    when: always
    paths:
      - Perf_Regression_Detector/*.benchmark.json
```

### Azure Pipelines
```yaml
- task: DotNetCoreCLI@2
  displayName: 'Run Performance Benchmarks'
  inputs:
    command: 'run'
    projects: 'Perf_Regression_Detector/PerfReg/PerfReg.csproj'
    arguments: 'run ../MyApp/bin/Release/net8.0/MyApp.dll --runs 10 --fail-on-regression'
```

## Exit Code Behavior

| Scenario | Exit Code | Description |
|----------|-----------|-------------|
| Success | 0 | Benchmark completed, no regression |
| Regression (default) | 0 | Regression detected, but not failing |
| Regression (--fail-on-regression) | 1 | Regression detected, fail build |
| Regression (config FailOnRegression: true) | 1 | Regression detected, fail build |
| Error | 1 | Process failed, invalid args, etc. |

## Troubleshooting

### "Failed to start process"
- Check that the binary path is correct
- Ensure the binary has execute permissions
- Use relative or absolute paths
- For .NET apps, use the .dll or .exe file

### "No benchmark history found"
- Run at least one benchmark first
- Check you're in the correct directory
- Verify `.benchmark.json` files exist

### High variance in results
- Increase number of runs (--runs 30+)
- Add warmup runs (--warmup 5+)
- Close other applications
- Run on dedicated hardware
- Check system load and cooling

### Memory measurements seem wrong
- Memory is peak working set, not actual allocated
- .NET apps may pre-allocate memory
- GC may not have run yet
- Consider multiple runs for average

### Charts not displaying correctly
- Ensure terminal width is at least 80 characters
- Check ANSI color support (works on most modern terminals)
- On Windows, use Windows Terminal or PowerShell 7+

## File Format

Results are stored in `<program-name>.benchmark.json`:

```json
{
  "ProgramName": "MyApp",
  "Results": [
    {
      "CommitHash": "abc1234",
      "Timestamp": "2026-02-01T12:00:00.000Z",
      "RuntimeMs": 245.67,
      "PeakMemoryBytes": 52428800,
      "Gen0Collections": 5,
      "Gen1Collections": 2,
      "Gen2Collections": 0,
      "Statistics": {
        "TotalRuns": 10,
        "WarmupRuns": 2,
        "Runtime": {
          "Mean": 245.67,
          "Median": 243.21,
          "StdDev": 12.34,
          "Min": 230.45,
          "Max": 265.89,
          "P50": 243.21,
          "P95": 261.45,
          "P99": 264.23
        },
        "Memory": {
          "Mean": 52428800,
          "Median": 52000000,
          "StdDev": 1048576,
          "Min": 51200000,
          "Max": 54000000,
          "P50": 52000000,
          "P95": 53500000,
          "P99": 53900000
        },
        "GarbageCollection": {
          "Gen0": { "Mean": 5.2, "Median": 5, "Min": 4, "Max": 7 },
          "Gen1": { "Mean": 2.1, "Median": 2, "Min": 1, "Max": 3 },
          "Gen2": { "Mean": 0.3, "Median": 0, "Min": 0, "Max": 1 }
        }
      }
    }
  ]
}
```

Baselines are stored in `<program-name>.baseline.json` with the same format.

## Tips & Tricks

### Compare Against Baseline
```bash
# Run baseline on main branch
git checkout main
dotnet run --project PerfReg run MyApp.exe --runs 10
dotnet run --project PerfReg baseline set

# Switch to feature branch
git checkout feature-xyz
dotnet run --project PerfReg run MyApp.exe --runs 10

# Compare shows difference
dotnet run --project PerfReg baseline compare
```

### Track Performance Over Time
```bash
# Run benchmarks regularly and view trends
dotnet run --project PerfReg run MyApp.exe --runs 10

# After multiple runs, visualize trends
dotnet run --project PerfReg trend --window 20
```

### Analyze Historical Performance
```bash
# Find when performance started degrading
git log --oneline | head -10  # Get recent commits
dotnet run --project PerfReg compare-historical <old-commit>
```

### Export for Analysis
```bash
# Export to CSV for spreadsheet analysis
dotnet run --project PerfReg export | \
  jq -r '.Results[] | [.Timestamp, .RuntimeMs, .PeakMemoryBytes] | @csv' > results.csv
```

### Dashboard Integration
```bash
# Send metrics to time-series database
dotnet run --project PerfReg export | \
  jq '.Results[] | {time: .Timestamp, runtime: .RuntimeMs, memory: .PeakMemoryBytes}' | \
  while read line; do
    curl -XPOST http://metrics-server/api/benchmarks -d "$line"
  done
```

## Advanced Usage

### Custom Thresholds Per Metric

Edit `.perfreg.json`:
```json
{
  "Thresholds": {
    "RuntimePercent": 3.0,    // Strict: 3% threshold
    "MemoryPercent": 10.0,    // Relaxed: 10% threshold
    "GcGen0Percent": 20.0,    // Very relaxed
    "GcGen1Percent": 15.0,
    "GcGen2Percent": 5.0      // Strict: Gen2 is expensive
  }
}
```

### Batch Benchmarking
```bash
# Test multiple scenarios
for version in v1 v2 v3 v4; do
  dotnet run --project PerfReg run TestProgram.exe $version --runs 5
done

# View combined trends
dotnet run --project PerfReg trend
```

### Commit History to Git (Optional)
```bash
# Track performance over time in git
git add *.benchmark.json
git commit -m "chore: update performance benchmarks"
```

## Feature Summary

| Feature | Sprint | Description |
|---------|--------|-------------|
| Multiple runs | 1 | Run benchmarks N times for statistics |
| Statistics | 1 | Mean, median, stddev, min, max |
| Warmup | 1 | JIT warmup before measurement |
| Configuration | 1 | `.perfreg.json` for defaults |
| Exit codes | 2 | Fail CI builds on regression |
| Baselines | 2 | Stable reference points |
| JSON export | 2 | Structured data for tooling |
| Markdown output | 2 | GitHub Actions integration |
| Percentiles | 3 | P50, P95, P99 tracking |
| Trend analysis | 3 | Linear regression on metrics |
| Terminal charts | 3 | ASCII line charts, sparklines |
| Histograms | 3 | Distribution visualization |
| Historical compare | 3 | Compare any two commits |

## Support

- **View all commands**: `dotnet run --project PerfReg help`
- **Version info**: `dotnet run --project PerfReg version`
- **Documentation**: See SPRINT1_SUMMARY.md, SPRINT2_SUMMARY.md, SPRINT3_SUMMARY.md
- **Roadmap**: See ROADMAP.md for future features

---

**Pro Tip:** Start with `--runs 10 --warmup 3` for most benchmarks. Increase runs to 50+ if you see high variance or need accurate percentiles.

**Version:** 3.0.0 (Sprint 1 + Sprint 2 + Sprint 3 Complete)
