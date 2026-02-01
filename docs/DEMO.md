# PerfReg Demo Guide

Complete walkthrough demonstrating all features of the Performance Regression Detector.

## Prerequisites

Make sure you're in the `Perf_Regression_Detector` directory:
```bash
cd Perf_Regression_Detector
```

## Demo 1: Basic Benchmarking

Run a simple benchmark to establish baseline performance.

```bash
perfreg run DemoApp/bin/Debug/net8.0/DemoApp.exe fast
```

**What you'll see:**
- Runtime in milliseconds
- Peak memory usage
- GC collection counts
- Git commit hash

## Demo 2: Multiple Runs with Statistics

Run the benchmark 10 times to get statistical analysis.

```bash
perfreg run DemoApp/bin/Debug/net8.0/DemoApp.exe fast --runs 10
```

**What you'll see:**
- Mean, median, standard deviation
- Min/max values
- **P50, P95, P99 percentiles** - tail latency metrics

## Demo 3: Warmup Runs for Accurate Results

Use warmup runs to let JIT compilation stabilize before measuring.

```bash
perfreg run DemoApp/bin/Debug/net8.0/DemoApp.exe fast --runs 10 --warmup 3
```

**What you'll see:**
- 3 warmup iterations (excluded from stats)
- 10 measured runs with statistics

## Demo 4: Compare Performance

Run the benchmark twice with different scenarios and compare.

```bash
# First run - fast scenario
perfreg run DemoApp/bin/Debug/net8.0/DemoApp.exe fast

# Second run - slow scenario
perfreg run DemoApp/bin/Debug/net8.0/DemoApp.exe slow

# Compare the results
perfreg compare
```

**What you'll see:**
- Side-by-side comparison
- Percentage change for each metric
- Warning symbols for regressions (>5% degradation)

## Demo 5: View History

See all benchmark runs for your application.

```bash
perfreg history
```

**What you'll see:**
- All programs benchmarked
- Recent runs for each program
- Timestamps and commit hashes

## Demo 6: Trend Analysis with Charts

This is where it gets visually impressive! Generate beautiful ASCII charts.

```bash
# Run multiple times to build history
perfreg run DemoApp/bin/Debug/net8.0/DemoApp.exe fast --runs 5
perfreg run DemoApp/bin/Debug/net8.0/DemoApp.exe fast --runs 5
perfreg run DemoApp/bin/Debug/net8.0/DemoApp.exe fast --runs 5
perfreg run DemoApp/bin/Debug/net8.0/DemoApp.exe slow --runs 5
perfreg run DemoApp/bin/Debug/net8.0/DemoApp.exe slow --runs 5

# Show trend analysis
perfreg trend DemoApp
```

**What you'll see:**
- **Line charts** showing performance over time
- **Sparklines** - compact inline trend indicators
- **Histograms** - distribution visualization
- **Trend direction** - Improving/Stable/Degrading
- **Percentage change** over the analysis window

## Demo 7: Variable Performance (Percentile Showcase)

Demonstrate why P95/P99 metrics matter.

```bash
perfreg run DemoApp/bin/Debug/net8.0/DemoApp.exe variable --runs 20
```

**What you'll see:**
- High variance in runtime
- P99 significantly higher than median
- Shows tail latency issues that mean values hide

## Demo 8: Baseline Management

Set a performance baseline and compare against it.

```bash
# Set baseline with fast scenario
perfreg run DemoApp/bin/Debug/net8.0/DemoApp.exe fast --runs 5
perfreg baseline set

# Show current baseline
perfreg baseline show

# Run slower scenario
perfreg run DemoApp/bin/Debug/net8.0/DemoApp.exe slow --runs 5

# Compare against baseline
perfreg baseline compare
```

**What you'll see:**
- Baseline stored as reference point
- Comparison showing regression vs baseline
- Warning indicators for degraded metrics

## Demo 9: Historical Comparison

Compare performance against a specific git commit.

```bash
# Get current commit
git log --oneline -5

# Run current benchmark
perfreg run DemoApp/bin/Debug/net8.0/DemoApp.exe slow

# Compare against older commit (replace abc1234 with actual commit hash)
perfreg compare-historical abc1234
```

**What you'll see:**
- Comparison between two specific commits
- Trend sparkline showing progression
- Performance delta over time

## Demo 10: Memory-Intensive Workload

Show GC collection tracking.

```bash
perfreg run DemoApp/bin/Debug/net8.0/DemoApp.exe memory --runs 5
```

**What you'll see:**
- High memory usage (50MB+)
- Multiple GC collections (Gen 0, 1, 2)
- Memory statistics across runs

## Demo 11: CI/CD Integration

Simulate a CI/CD pipeline with fail-on-regression.

```bash
# Set a good baseline
perfreg run DemoApp/bin/Debug/net8.0/DemoApp.exe fast --runs 5
perfreg baseline set

# Run with regression detection
perfreg run DemoApp/bin/Debug/net8.0/DemoApp.exe slow --runs 5 --fail-on-regression

# Check exit code
echo $LASTEXITCODE
```

**What you'll see:**
- Warning about regression
- Exit code 1 (failure)
- Perfect for blocking CI/CD builds

## Demo 12: JSON Export

Export data for dashboards or custom tooling.

```bash
perfreg export DemoApp > demo-results.json
```

**What you'll see:**
- Complete benchmark history in JSON format
- Machine-readable for integration with other tools

## Demo 13: Configuration File

Create and use a configuration file for project-specific settings.

```bash
# Generate default config
perfreg config

# Edit .perfreg.json to customize thresholds
# Then run benchmarks - settings automatically applied
perfreg run DemoApp/bin/Debug/net8.0/DemoApp.exe fast
```

**What you'll see:**
- `.perfreg.json` created with defaults
- Custom thresholds for regressions
- Default runs/warmup settings

## Demo 14: Complete Workflow

Full development workflow showing real-world usage.

```bash
# 1. Initial benchmark (feature branch)
perfreg run DemoApp/bin/Debug/net8.0/DemoApp.exe fast --runs 10
perfreg baseline set

# 2. Make code changes (simulated by switching scenario)
# Simulate optimization
perfreg run DemoApp/bin/Debug/net8.0/DemoApp.exe fast --runs 10

# 3. Compare against baseline
perfreg baseline compare

# 4. View trends
perfreg trend DemoApp

# 5. Export for records
perfreg export DemoApp > performance-report.json
```

## Quick Demo Script

For a fast 2-minute demo showing the highlights:

```bash
# Clean slate
perfreg clear

# Run 1: Basic benchmark
perfreg run DemoApp/bin/Debug/net8.0/DemoApp.exe fast

# Run 2: With statistics
perfreg run DemoApp/bin/Debug/net8.0/DemoApp.exe fast --runs 10

# Run 3: Create regression
perfreg run DemoApp/bin/Debug/net8.0/DemoApp.exe slow --runs 10

# Compare
perfreg compare

# Run a few more for trend data
perfreg run DemoApp/bin/Debug/net8.0/DemoApp.exe fast
perfreg run DemoApp/bin/Debug/net8.0/DemoApp.exe slow
perfreg run DemoApp/bin/Debug/net8.0/DemoApp.exe fast

# Show trends with beautiful charts
perfreg trend DemoApp
```

## Pro Tips

1. **Terminal Width**: Use a terminal at least 80 characters wide for best chart rendering
2. **Color Support**: Use a modern terminal (Windows Terminal, iTerm2, etc.) for color-coded output
3. **Data Reset**: Use `perfreg clear` to start fresh between demos
4. **Baseline Management**: Clear baselines with `perfreg baseline clear`

## Feature Showcase Summary

- **Basic Benchmarking** - Runtime, memory, GC tracking
- **Statistical Analysis** - Mean, median, stddev, min/max
- **Percentile Tracking** - P50, P95, P99 for tail latency
- **Trend Analysis** - Linear regression over time
- **Terminal Charts** - Line charts, sparklines, histograms
- **Baseline Management** - Stable reference points
- **Historical Comparison** - Compare any two commits
- **CI/CD Integration** - Exit codes for automated pipelines
- **JSON Export** - Machine-readable output
- **Configuration** - Project-specific settings
- **Git Integration** - Automatic commit tracking

## Most Impressive Features for Presentation

1. **Trend command with ASCII charts** - Visually stunning
2. **P95/P99 percentile tracking** - Professional-grade metrics
3. **Regression detection with warnings** - Automated quality gates
4. **Historical comparison** - Deep analysis capability
5. **Statistical confidence** - Multiple runs with variance

Enjoy demoing PerfReg!
