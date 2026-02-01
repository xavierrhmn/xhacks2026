using PerfReg.Models;

namespace PerfReg.Analysis;

public interface IResultAnalyzer
{
    ComparisonReport Compare(BenchmarkResult current, BenchmarkResult previous);
}

public record ComparisonReport(
    BenchmarkResult Current,
    BenchmarkResult Previous,
    List<MetricComparison> Metrics
);

public record MetricComparison(
    string Name,
    double CurrentValue,
    double PreviousValue,
    double PercentageChange,
    string Unit,
    bool IsRegression,
    ChangeDirection Direction
);

public enum ChangeDirection
{
    Improved,
    Degraded,
    Unchanged
}
