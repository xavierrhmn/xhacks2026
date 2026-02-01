using PerfReg.Analysis;
using PerfReg.Models;

namespace PerfReg.Reporting;

public class ConsoleReporter : IReporter
{
    public void ShowResult(BenchmarkResult result)
    {
        Console.WriteLine($"\n✓ Benchmark complete!");

        if (result.Statistics != null)
        {
            var stats = result.Statistics;
            Console.WriteLine($"\nResults ({stats.TotalRuns} run(s), {stats.WarmupRuns} warmup(s)):");
            Console.WriteLine($"  Runtime:     {result.RuntimeMs:F2}ms (±{stats.Runtime.StdDev:F2}ms)");
            Console.WriteLine($"               [min: {stats.Runtime.Min:F2}ms, median: {stats.Runtime.Median:F2}ms, max: {stats.Runtime.Max:F2}ms]");

            if (stats.Runtime.P95.HasValue && stats.Runtime.P99.HasValue)
            {
                Console.WriteLine($"               [p50: {stats.Runtime.P50:F2}ms, p95: {stats.Runtime.P95:F2}ms, p99: {stats.Runtime.P99:F2}ms]");
            }

            Console.WriteLine($"  Peak Memory: {result.PeakMemoryBytes / 1024.0 / 1024.0:F2}MB (±{stats.Memory.StdDev / 1024.0 / 1024.0:F2}MB)");
            Console.WriteLine($"               [min: {stats.Memory.Min / 1024.0 / 1024.0:F2}MB, median: {stats.Memory.Median / 1024.0 / 1024.0:F2}MB, max: {stats.Memory.Max / 1024.0 / 1024.0:F2}MB]");

            if (stats.Memory.P95.HasValue && stats.Memory.P99.HasValue)
            {
                Console.WriteLine($"               [p50: {stats.Memory.P50 / 1024.0 / 1024.0:F2}MB, p95: {stats.Memory.P95 / 1024.0 / 1024.0:F2}MB, p99: {stats.Memory.P99 / 1024.0 / 1024.0:F2}MB]");
            }

            Console.WriteLine($"  GC Gen0/1/2: {result.Gen0Collections}/{result.Gen1Collections}/{result.Gen2Collections}");
        }
        else
        {
            Console.WriteLine($"  Runtime: {result.RuntimeMs:F2}ms");
            Console.WriteLine($"  Peak Memory: {result.PeakMemoryBytes / 1024.0 / 1024.0:F2}MB");
            Console.WriteLine($"  GC Gen0/1/2: {result.Gen0Collections}/{result.Gen1Collections}/{result.Gen2Collections}");
        }
    }

    public void ShowComparison(ComparisonReport report)
    {
        Console.WriteLine("=== Performance Comparison ===");
        Console.WriteLine($"Current:  {report.Current.Timestamp:yyyy-MM-dd HH:mm:ss} ({report.Current.CommitHash})");
        Console.WriteLine($"Previous: {report.Previous.Timestamp:yyyy-MM-dd HH:mm:ss} ({report.Previous.CommitHash})");
        Console.WriteLine();

        foreach (var metric in report.Metrics)
        {
            ShowMetricComparison(metric);
        }
    }

    public void ShowHistory(BenchmarkHistory history)
    {
        Console.WriteLine($"\n=== {history.ProgramName} ===");
        Console.WriteLine($"Total runs: {history.Results.Count}");

        if (history.Results.Count > 0)
        {
            Console.WriteLine("\nRecent runs:");
            foreach (var result in history.Results.TakeLast(5).Reverse())
            {
                var statsInfo = result.Statistics != null ? $" ({result.Statistics.TotalRuns}x)" : "";
                Console.WriteLine($"  {result.Timestamp:yyyy-MM-dd HH:mm:ss} - {result.RuntimeMs:F2}ms{statsInfo} - {result.CommitHash[..Math.Min(8, result.CommitHash.Length)]}");
            }
        }
    }

    private void ShowMetricComparison(MetricComparison metric)
    {
        var changeStr = $"{(metric.PercentageChange >= 0 ? "+" : "")}{metric.PercentageChange:F1}%";

        var symbol = metric.Direction switch
        {
            ChangeDirection.Degraded when metric.IsRegression => "⚠️ ",
            ChangeDirection.Degraded => "↑",
            ChangeDirection.Improved => "↓",
            _ => "→"
        };

        Console.WriteLine($"{metric.Name,-15}: {metric.CurrentValue:F2}{metric.Unit,-4} ({changeStr,8}) {symbol}");
    }
}
