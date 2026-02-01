using PerfReg.Models;

namespace PerfReg.Analysis;

public class ComparisonAnalyzer : IResultAnalyzer
{
    private readonly double _regressionThreshold;

    public ComparisonAnalyzer(double regressionThreshold = 5.0)
    {
        _regressionThreshold = regressionThreshold;
    }

    public ComparisonReport Compare(BenchmarkResult current, BenchmarkResult previous)
    {
        var metrics = new List<MetricComparison>
        {
            CompareMetric("Runtime", current.RuntimeMs, previous.RuntimeMs, "ms", lowerIsBetter: true),
            CompareMetric("Peak Memory",
                current.PeakMemoryBytes / 1024.0 / 1024.0,
                previous.PeakMemoryBytes / 1024.0 / 1024.0,
                "MB",
                lowerIsBetter: true),
            CompareMetric("GC Gen0", current.Gen0Collections, previous.Gen0Collections, "", lowerIsBetter: true),
            CompareMetric("GC Gen1", current.Gen1Collections, previous.Gen1Collections, "", lowerIsBetter: true),
            CompareMetric("GC Gen2", current.Gen2Collections, previous.Gen2Collections, "", lowerIsBetter: true)
        };

        return new ComparisonReport(current, previous, metrics);
    }

    private MetricComparison CompareMetric(string name, double current, double previous, string unit, bool lowerIsBetter)
    {
        var percentageChange = previous == 0 ? 0 : ((current - previous) / previous) * 100.0;

        var direction = percentageChange switch
        {
            > 1 => lowerIsBetter ? ChangeDirection.Degraded : ChangeDirection.Improved,
            < -1 => lowerIsBetter ? ChangeDirection.Improved : ChangeDirection.Degraded,
            _ => ChangeDirection.Unchanged
        };

        var isRegression = lowerIsBetter
            ? percentageChange > _regressionThreshold
            : percentageChange < -_regressionThreshold;

        return new MetricComparison(
            Name: name,
            CurrentValue: current,
            PreviousValue: previous,
            PercentageChange: percentageChange,
            Unit: unit,
            IsRegression: isRegression,
            Direction: direction
        );
    }
}
