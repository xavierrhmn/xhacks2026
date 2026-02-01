# PerfReg - Performance Regression Detector

**A powerful, extensible performance regression detection tool for .NET applications**

[![Version](https://img.shields.io/badge/version-3.0.0-blue.svg)](https://github.com/yourusername/perfreg)
[![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)

## Features

### Core Capabilities
- **Automated Benchmarking** - Track runtime, memory, and GC collections
- **Statistical Analysis** - Multiple runs with mean, median, standard deviation
- **Percentile Tracking** - P50, P95, P99 for tail latency analysis
- **Regression Detection** - Automatic warnings for performance degradation
- **Git Integration** - Associate results with commit hashes

### Advanced Analysis
- **Trend Analysis** - Linear regression to detect gradual degradation
- **Terminal Charts** - Beautiful ASCII visualizations (line charts, sparklines, histograms)
- **Historical Comparison** - Compare against any previous commit
- **Baseline Management** - Set stable reference points for comparison

### Developer Experience
- **CI/CD Integration** - Exit codes and GitHub Actions support
- **JSON Export** - Machine-readable output for tooling
- **Configuration Files** - Project-specific settings with .perfreg.json
- **Multiple Output Formats** - Console, JSON, Markdown, GitHub Actions

## Visual Examples

### Terminal Charts
```
Runtime Trend (last 10 runs):

  335.12 ┤
       │                                                           ●
       │                                                          ─
       │●───                                   ●─                ─
       │    ──●            ●                  ─  ───            ─
       │       ──        ── ─                ─      ─●         ─
       │         ──     ─    ─              ─         ──       ─
       │           ── ──      ──           ─            ──    ─
       │             ●          ─         ─               ── ─
       │                         ─       ─                  ●
       │                          ●─────●
  232.23 └────────────────────────────────────────────────────────────
       First                                                  Last

Quick View:
  Runtime:  ▆▆▂▆▁▁▆▅▂█  314.2ms → 335.1ms
  Memory:   ▁▁▁▁▁▁▁█▁▁  24.5MB → 24.6MB
```

### Statistical Output
```
Results (5 run(s), 0 warmup(s)):
  Runtime:     233.88ms (±28.30ms)
               [min: 186.22ms, median: 242.54ms, max: 260.11ms]
               [p50: 242.54ms, p95: 257.46ms, p99: 259.58ms]
  Peak Memory: 20.87MB (±0.10MB)
               [min: 20.76MB, median: 20.89MB, max: 20.99MB]
               [p50: 20.89MB, p95: 20.98MB, p99: 20.99MB]
```

### Trend Analysis
```
╔════════════════════════════════════════════════════════════════╗
║                    Trend Analysis: TestProgram                 ║
╚════════════════════════════════════════════════════════════════╝

Overall Trend: ⚠️ Degrading
Analysis Window: Last 10 runs

Metric Trends:
  ⚠️ Runtime        :      +6.7% over 10 runs
  → Memory         :      +0.3% over 10 runs
```

## Quick Start

### Run a benchmark
```bash
dotnet run --project PerfReg run MyApp.exe
```

### Run with multiple iterations for statistics
```bash
dotnet run --project PerfReg run MyApp.exe --runs 10 --warmup 2
```

### Compare with previous run
```bash
dotnet run --project PerfReg compare
```

### View performance trends with charts
```bash
dotnet run --project PerfReg trend
```

### Set a baseline and fail on regression (CI/CD)
```bash
dotnet run --project PerfReg baseline set
dotnet run --project PerfReg run MyApp.exe --fail-on-regression
```

## Installation

### Prerequisites
- .NET 8.0 SDK or later
- Git (optional, for commit tracking)

### Clone and Build
```bash
git clone https://github.com/yourusername/perfreg.git
cd perfreg/Perf_Regression_Detector
dotnet build
```

### Create Configuration (Optional)
```bash
dotnet run --project PerfReg config
```

This creates `.perfreg.json`:
```json
{
  "defaultRuns": 5,
  "defaultWarmupRuns": 2,
  "failOnRegression": false,
  "thresholds": {
    "runtimePercentage": 5.0,
    "memoryPercentage": 10.0
  }
}
```

## Commands

### Core Commands

#### `run <binary> [args...] [options]`
Run a benchmark and store results.

**Options:**
- `--runs N` - Run benchmark N times (default: 1)
- `--warmup N` - Run N warmup iterations (default: 0)
- `--fail-on-regression` - Exit with code 1 if regression detected

**Examples:**
```bash
# Single run
dotnet run --project PerfReg run MyApp.exe

# With arguments
dotnet run --project PerfReg run MyApp.exe arg1 arg2

# Multiple runs with warmup
dotnet run --project PerfReg run MyApp.exe --runs 10 --warmup 3

# Fail on regression (for CI/CD)
dotnet run --project PerfReg run MyApp.exe --fail-on-regression
```

#### `compare`
Compare the last two benchmark runs.

```bash
dotnet run --project PerfReg compare
```

**Output:**
```
=== Performance Comparison ===
Current:  2026-01-31 10:30:15 (abc1234)
Previous: 2026-01-31 09:45:22 (def5678)

Runtime        : 245.32ms   (  +12.5%) ⚠️
Peak Memory    : 21.45MB   (   +2.1%) →
```

#### `history`
Show performance history for all programs.

```bash
dotnet run --project PerfReg history
```

#### `trend [program] [--window N]`
Show performance trends with charts.

**Options:**
- `--window N` - Number of runs to analyze (default: 10)

```bash
# Show trends for all programs (last 10 runs)
dotnet run --project PerfReg trend

# Specific program with custom window
dotnet run --project PerfReg trend MyApp --window 20
```

#### `compare-historical <commit>`
Compare current performance against a specific commit.

```bash
dotnet run --project PerfReg compare-historical abc1234
```

**Output:**
```
═══ Historical Comparison: TestProgram ═══

Comparing:
  Current:  8101732 (2026-01-31 01:41:57)
  Target:   8101732 (2026-01-31 01:41:15)

Runtime        : 335.12ms   (  +43.3%) ⚠️
Peak Memory    : 24.61MB   (  +17.9%) ⚠️

═══ Trend Over 6 Commits ═══

  Runtime trend: ▁▁▆▅▂█
```

### Baseline Commands

#### `baseline set`
Set current run as baseline for future comparisons.

```bash
dotnet run --project PerfReg baseline set
```

#### `baseline compare`
Compare latest run against baseline.

```bash
dotnet run --project PerfReg baseline compare
```

#### `baseline show`
Display current baseline.

```bash
dotnet run --project PerfReg baseline show
```

#### `baseline clear`
Remove baseline.

```bash
dotnet run --project PerfReg baseline clear
```

### Utility Commands

#### `export [program]`
Export benchmark data as JSON.

```bash
# Export all programs
dotnet run --project PerfReg export > results.json

# Export specific program
dotnet run --project PerfReg export MyApp > myapp-results.json
```

#### `clear`
Clear all benchmark history.

```bash
dotnet run --project PerfReg clear
```

#### `config`
Create default `.perfreg.json` configuration file.

```bash
dotnet run --project PerfReg config
```

#### `help`
Show help menu.

```bash
dotnet run --project PerfReg help
```

#### `version`
Show version information.

```bash
dotnet run --project PerfReg version
```

## Tracked Metrics

- **Runtime** - Execution time in milliseconds
- **Peak Memory** - Maximum memory usage in MB
- **GC Collections** - Gen 0, 1, and 2 collection counts
- **Git Commit Hash** - Associated commit (if available)
- **Percentiles** - P50 (median), P95, P99 for tail latency

## Use Cases

### 1. Development Workflow
```bash
# Before making changes
dotnet run --project PerfReg baseline set

# Make code changes...

# After changes
dotnet run --project PerfReg run MyApp.exe --runs 10
dotnet run --project PerfReg baseline compare
```

### 2. CI/CD Pipeline
```yaml
# GitHub Actions example
- name: Run Performance Tests
  run: dotnet run --project PerfReg run MyApp.exe --runs 5 --fail-on-regression
```

### 3. Trend Monitoring
```bash
# Regular monitoring
dotnet run --project PerfReg run MyApp.exe --runs 5
dotnet run --project PerfReg trend

# Check for gradual degradation
# If Overall Trend shows "Degrading", investigate!
```

### 4. Release Validation
```bash
# Compare against release branch
dotnet run --project PerfReg compare-historical <release-commit>

# Ensure no significant regression before release
```

### 5. Tail Latency Investigation
```bash
# Run multiple times to get percentiles
dotnet run --project PerfReg run MyApp.exe --runs 50

# Check P95/P99 values
# If P99 >> median, you have tail latency issues
```

## Architecture

### Modular Design
```
PerfReg/
├── Models/              - Data structures
│   ├── BenchmarkResult.cs
│   └── BenchmarkHistory.cs
├── Core/                - Benchmark execution
│   ├── BenchmarkRunner.cs
│   └── ProcessMetricsCollector.cs
├── Storage/             - Persistence layer
│   └── JsonHistoryStorage.cs
├── Analysis/            - Result analysis
│   ├── ComparisonAnalyzer.cs
│   └── TrendAnalyzer.cs
├── Reporting/           - Output formatting
│   ├── ConsoleReporter.cs
│   ├── JsonReporter.cs
│   ├── MarkdownReporter.cs
│   ├── GitHubActionsReporter.cs
│   └── TerminalChartRenderer.cs
├── Commands/            - CLI commands
│   ├── RunCommand.cs
│   ├── CompareCommand.cs
│   ├── TrendCommand.cs
│   └── BaselineCommand.cs
├── Configuration/       - Config management
│   └── ConfigLoader.cs
└── Utilities/           - Helper functions
    ├── GitHelper.cs
    └── StatisticsCalculator.cs
```

### Extensibility
- **Interface-based design** - Easy to add new reporters, storage backends, analyzers
- **Command pattern** - Simple to add new CLI commands
- **SOLID principles** - Single responsibility, dependency injection

## CI/CD Integration

### GitHub Actions
```yaml
name: Performance Tests

on: [push, pull_request]

jobs:
  benchmark:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3

      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'

      - name: Build Test Application
        run: dotnet build MyApp/MyApp.csproj

      - name: Run Performance Benchmark
        run: |
          dotnet run --project PerfReg run MyApp/bin/Debug/net8.0/MyApp.exe --runs 10 --fail-on-regression
```

### Exit Codes
- `0` - Success (no regression)
- `1` - Failure (regression detected or error)

### Markdown Reports
```bash
# Generate markdown report
dotnet run --project PerfReg run MyApp.exe --runs 5
dotnet run --project PerfReg compare --format markdown > report.md
```

## Development Sprints

### Sprint 1: Foundation
- Multiple benchmark runs with statistics
- Configuration file support
- Warmup runs for JIT compilation
- **Status:** Complete

### Sprint 2: CI/CD Integration
- Exit codes with `--fail-on-regression`
- Baseline management
- JSON export
- GitHub Actions integration
- **Status:** Complete

### Sprint 3: Advanced Analysis
- Trend analysis with linear regression
- Terminal charts (line charts, sparklines, histograms)
- Percentile tracking (P50, P95, P99)
- Historical comparison
- **Status:** Complete

### Future Enhancements
- HTML reports with interactive charts
- CPU profiling integration
- Web dashboard for browsing history
- Export charts as PNG/SVG
- Slack/Discord notifications

## Configuration Reference

### .perfreg.json
```json
{
  "defaultRuns": 5,
  "defaultWarmupRuns": 2,
  "failOnRegression": false,
  "thresholds": {
    "runtimePercentage": 5.0,
    "memoryPercentage": 10.0
  }
}
```

**Fields:**
- `defaultRuns` - Default number of benchmark runs
- `defaultWarmupRuns` - Default number of warmup iterations
- `failOnRegression` - Exit with code 1 on regression (can override with --fail-on-regression flag)
- `thresholds.runtimePercentage` - Runtime regression threshold (%)
- `thresholds.memoryPercentage` - Memory regression threshold (%)

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Acknowledgments

Built for XHacks 2026 - Demonstrating modern .NET performance monitoring techniques.

## Documentation

- [USAGE_GUIDE.md](USAGE_GUIDE.md) - Comprehensive usage guide
- [ARCHITECTURE.md](ARCHITECTURE.md) - Architecture documentation
- [ROADMAP.md](ROADMAP.md) - Development roadmap
- [SPRINT1_SUMMARY.md](SPRINT1_SUMMARY.md) - Sprint 1 details
- [SPRINT2_SUMMARY.md](SPRINT2_SUMMARY.md) - Sprint 2 details
- [SPRINT3_SUMMARY.md](SPRINT3_SUMMARY.md) - Sprint 3 details

## Troubleshooting

### "Command not found"
Make sure you're running from the correct directory:
```bash
cd Perf_Regression_Detector
dotnet run --project PerfReg <command>
```

### "File not found" when running benchmark
Provide the full path to the executable:
```bash
dotnet run --project PerfReg run /full/path/to/MyApp.exe
```

### Charts not displaying correctly
- Ensure your terminal supports ANSI colors
- Use a terminal width of at least 80 characters
- On Windows, use Windows Terminal or PowerShell 7+

## Tips and Best Practices

1. **Use warmup runs** for accurate measurements of JIT-compiled code
2. **Run multiple iterations** (10+) for reliable statistics
3. **Set baselines** before major changes to track impact
4. **Monitor trends** regularly to catch gradual degradation
5. **Check P95/P99** for tail latency issues
6. **Integrate with CI/CD** to prevent regressions from merging
