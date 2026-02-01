# Performance Regression Detector - Architecture

## Overview
This document outlines the proposed multi-file architecture for the Performance Regression Detector. The design emphasizes extensibility, separation of concerns, and testability.

## Directory Structure

```
PerfReg/
├── Models/
│   ├── BenchmarkResult.cs          # Data model for single benchmark result
│   └── BenchmarkHistory.cs         # Data model for benchmark history collection
│
├── Core/
│   ├── IBenchmarkRunner.cs         # Interface: Execute and monitor processes
│   ├── BenchmarkRunner.cs          # Implementation: Process execution & monitoring
│   ├── IMetricsCollector.cs        # Interface: Collect performance metrics
│   └── ProcessMetricsCollector.cs  # Implementation: Process-level metrics (CPU, memory, GC)
│
├── Storage/
│   ├── IHistoryStorage.cs          # Interface: Load/save benchmark history
│   └── JsonHistoryStorage.cs       # Implementation: JSON file-based storage
│
├── Analysis/
│   ├── IResultAnalyzer.cs          # Interface: Analyze benchmark results
│   └── ComparisonAnalyzer.cs       # Implementation: Compare two results, detect regressions
│
├── Reporting/
│   ├── IReporter.cs                # Interface: Format and display results
│   └── ConsoleReporter.cs          # Implementation: Console output with colors
│
├── Commands/
│   ├── ICommand.cs                 # Interface: CLI command pattern
│   ├── RunCommand.cs               # Command: Run benchmark
│   ├── CompareCommand.cs           # Command: Compare last two runs
│   ├── HistoryCommand.cs           # Command: Show full history
│   └── ClearCommand.cs             # Command: Clear history
│
├── Utilities/
│   └── GitHelper.cs                # Git operations (commit hash extraction)
│
├── HelpMenu.cs                     # Console UI for help display (existing)
└── Program.cs                      # Entry point & command routing

```

## Component Responsibilities

### Models/
**Purpose**: Data transfer objects and domain models

- `BenchmarkResult.cs` - Immutable record containing:
  - CommitHash, Timestamp, RuntimeMs, PeakMemoryBytes
  - Gen0/1/2 Collections

- `BenchmarkHistory.cs` - Collection wrapper:
  - ProgramName, List<BenchmarkResult>

### Core/
**Purpose**: Benchmark execution and metrics collection

- `IBenchmarkRunner.cs`
  ```csharp
  Task<BenchmarkResult> RunAsync(string binary, string[] args)
  ```
  - Defines contract for executing benchmarks

- `BenchmarkRunner.cs`
  - Starts and monitors external processes
  - Orchestrates metrics collection
  - Returns complete BenchmarkResult

- `IMetricsCollector.cs`
  ```csharp
  void StartCollection()
  Task<MetricsData> StopCollectionAsync()
  ```
  - Extensible metrics collection interface

- `ProcessMetricsCollector.cs`
  - Monitors: runtime, memory, GC collections
  - Polls process stats during execution

**Extensibility**: Add `NetworkMetricsCollector`, `DiskIOCollector`, etc.

### Storage/
**Purpose**: Persist and retrieve benchmark history

- `IHistoryStorage.cs`
  ```csharp
  BenchmarkHistory Load(string programName)
  void Save(BenchmarkHistory history)
  IEnumerable<string> ListPrograms()
  void Clear()
  ```

- `JsonHistoryStorage.cs`
  - File naming: `{programName}.benchmark.json`
  - JSON serialization with System.Text.Json

**Extensibility**: Add `SqliteStorage`, `PostgresStorage`, `CloudStorage`

### Analysis/
**Purpose**: Interpret and compare benchmark results

- `IResultAnalyzer.cs`
  ```csharp
  AnalysisReport Compare(BenchmarkResult current, BenchmarkResult baseline)
  IEnumerable<Regression> DetectRegressions(BenchmarkHistory history)
  ```

- `ComparisonAnalyzer.cs`
  - Calculates percentage changes
  - Applies threshold rules (>5% = warning)
  - Returns structured analysis data

**Extensibility**: Add `TrendAnalyzer`, `StatisticalAnalyzer`, `MLRegressionPredictor`

### Reporting/
**Purpose**: Format and display results to users

- `IReporter.cs`
  ```csharp
  void ShowResult(BenchmarkResult result)
  void ShowComparison(BenchmarkResult current, BenchmarkResult previous)
  void ShowHistory(BenchmarkHistory history)
  ```

- `ConsoleReporter.cs`
  - Colored console output
  - Formatted tables and metrics
  - Warning symbols (⚠️, ↑, ↓, →)

**Extensibility**: Add `JsonReporter`, `HtmlReporter`, `SlackReporter`

### Commands/
**Purpose**: Implement CLI command pattern

- `ICommand.cs`
  ```csharp
  Task<int> ExecuteAsync(string[] args)
  string Name { get; }
  string Description { get; }
  ```

- Each command is self-contained and composable:
  - `RunCommand` - Coordinates runner, storage, reporter
  - `CompareCommand` - Uses storage, analyzer, reporter
  - `HistoryCommand` - Uses storage, reporter
  - `ClearCommand` - Uses storage

**Benefits**:
- Easy to add new commands
- Testable in isolation
- Clear dependencies

### Utilities/
**Purpose**: Cross-cutting concerns and helpers

- `GitHelper.cs`
  - `GetCurrentCommitHash()` - Extract git commit
  - `GetBranchName()` - Future extension
  - `IsDirty()` - Future extension

## Extensibility Points

### 1. Metrics Collection
Add new metrics by implementing `IMetricsCollector`:
```csharp
public class NetworkMetricsCollector : IMetricsCollector
{
    // Track bytes sent/received during benchmark
}
```

### 2. Storage Backends
Switch storage by implementing `IHistoryStorage`:
```csharp
public class SqliteStorage : IHistoryStorage
{
    // Store in SQLite for better querying
}
```

### 3. Analysis Algorithms
Add new analysis by implementing `IResultAnalyzer`:
```csharp
public class TrendAnalyzer : IResultAnalyzer
{
    // Detect long-term performance trends
}
```

### 4. Output Formats
Add new reporters by implementing `IReporter`:
```csharp
public class SlackReporter : IReporter
{
    // Post results to Slack channel
}
```

### 5. Commands
Add new commands by implementing `ICommand`:
```csharp
public class ExportCommand : ICommand
{
    // Export history to CSV/Excel
}
```

## Dependency Flow

```
Program.cs (entry point)
    ↓
Commands/ (orchestration)
    ↓
Core/ (execution) ←→ Utilities/ (helpers)
    ↓
Storage/ (persistence)
    ↓
Analysis/ (interpretation)
    ↓
Reporting/ (presentation)
```

## Key Design Patterns

1. **Interface Segregation**: Small, focused interfaces
2. **Dependency Injection**: Classes depend on interfaces, not implementations
3. **Command Pattern**: Each CLI command is an object
4. **Strategy Pattern**: Swappable implementations (storage, reporting, analysis)
5. **Single Responsibility**: Each class has one reason to change

## Migration Strategy

The existing code can be refactored incrementally:

1. Extract Models → Create Models/ folder
2. Extract Storage → Create Storage/ folder with JsonHistoryStorage
3. Extract Core → Create BenchmarkRunner
4. Extract Reporting → Create ConsoleReporter
5. Extract Analysis → Create ComparisonAnalyzer
6. Extract Commands → Create command classes
7. Simplify Program.cs → Route to commands

## Benefits

- **Testability**: Mock interfaces for unit testing
- **Maintainability**: Clear separation of concerns
- **Extensibility**: Add features without modifying existing code
- **Flexibility**: Swap implementations (e.g., JSON → Database)
- **Readability**: Each file has a single, clear purpose
- **Scalability**: Easy to add new metrics, outputs, or analyses

## Testing Strategy

With this architecture, you can:
- Unit test analysis logic in isolation
- Mock storage for command tests
- Test reporters with fake data
- Integration test full command flow

## Future Extensions

Made easy by this architecture:
- Multiple metrics collectors running simultaneously
- CI/CD integration (GitHub Actions reporter)
- Historical trend analysis with statistical tests
- Performance budgets and automated alerts
- Multi-program comparisons
- Custom thresholds per metric
- Export to various formats (CSV, JSON, HTML reports)
