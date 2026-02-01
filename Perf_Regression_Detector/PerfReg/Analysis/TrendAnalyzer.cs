using PerfReg.Models;

namespace PerfReg.Analysis;

public class TrendAnalyzer
{
    public TrendReport AnalyzeTrend(List<BenchmarkResult> results, int windowSize = 10)
    {
        if (results.Count < 2)
        {
            return new TrendReport(
                TrendDirection.Stable,
                0,
                0,
                new List<TrendMetric>()
            );
        }

        var recentResults = results.TakeLast(Math.Min(windowSize, results.Count)).ToList();

        var runtimeTrend = CalculateTrend(recentResults.Select(r => r.RuntimeMs).ToList());
        var memoryTrend = CalculateTrend(recentResults.Select(r => (double)r.PeakMemoryBytes).ToList());

        var metrics = new List<TrendMetric>
        {
            new TrendMetric(
                "Runtime",
                runtimeTrend.Slope,
                runtimeTrend.ChangePercent,
                runtimeTrend.Direction
            ),
            new TrendMetric(
                "Memory",
                memoryTrend.Slope,
                memoryTrend.ChangePercent,
                memoryTrend.Direction
            )
        };

        // Overall trend is degrading if any major metric is degrading
        var overallDirection = metrics.Any(m => m.Direction == TrendDirection.Degrading)
            ? TrendDirection.Degrading
            : metrics.Any(m => m.Direction == TrendDirection.Improving)
                ? TrendDirection.Improving
                : TrendDirection.Stable;

        var avgSlope = metrics.Average(m => m.Slope);
        var avgChange = metrics.Average(m => m.ChangePercent);

        return new TrendReport(overallDirection, avgSlope, avgChange, metrics);
    }

    private (double Slope, double ChangePercent, TrendDirection Direction) CalculateTrend(List<double> values)
    {
        if (values.Count < 2)
            return (0, 0, TrendDirection.Stable);

        // Linear regression
        int n = values.Count;
        double sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0;

        for (int i = 0; i < n; i++)
        {
            double x = i;
            double y = values[i];
            sumX += x;
            sumY += y;
            sumXY += x * y;
            sumX2 += x * x;
        }

        double slope = (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);

        // Calculate percent change from first to last
        double firstValue = values.First();
        double lastValue = values.Last();
        double changePercent = firstValue != 0 ? ((lastValue - firstValue) / firstValue) * 100.0 : 0;

        // Determine direction
        TrendDirection direction;
        if (Math.Abs(changePercent) < 2.0)
            direction = TrendDirection.Stable;
        else if (changePercent > 0)
            direction = TrendDirection.Degrading; // Higher values = worse for performance
        else
            direction = TrendDirection.Improving;

        return (slope, changePercent, direction);
    }
}

public record TrendReport(
    TrendDirection OverallDirection,
    double AverageSlope,
    double AverageChangePercent,
    List<TrendMetric> Metrics
);

public record TrendMetric(
    string Name,
    double Slope,
    double ChangePercent,
    TrendDirection Direction
);

public enum TrendDirection
{
    Improving,
    Stable,
    Degrading
}
