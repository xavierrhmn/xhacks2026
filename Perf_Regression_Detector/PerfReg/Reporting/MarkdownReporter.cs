using PerfReg.Analysis;
using PerfReg.Models;

namespace PerfReg.Reporting;

public class MarkdownReporter : IReporter
{
    public void ShowResult(BenchmarkResult result)
    {
        Console.WriteLine("## Benchmark Results");
        Console.WriteLine();

        if (result.Statistics != null)
        {
            var stats = result.Statistics;
            Console.WriteLine($"**Configuration:** {stats.TotalRuns} run(s), {stats.WarmupRuns} warmup(s)");
            Console.WriteLine();
            Console.WriteLine("| Metric | Value | Range |");
            Console.WriteLine("|--------|-------|-------|");
            Console.WriteLine($"| Runtime | {result.RuntimeMs:F2}ms (±{stats.Runtime.StdDev:F2}ms) | {stats.Runtime.Min:F2}ms - {stats.Runtime.Max:F2}ms |");
            Console.WriteLine($"| Peak Memory | {result.PeakMemoryBytes / 1024.0 / 1024.0:F2}MB (±{stats.Memory.StdDev / 1024.0 / 1024.0:F2}MB) | {stats.Memory.Min / 1024.0 / 1024.0:F2}MB - {stats.Memory.Max / 1024.0 / 1024.0:F2}MB |");
            Console.WriteLine($"| GC Gen0 | {result.Gen0Collections} | {stats.GarbageCollection.Gen0.Min} - {stats.GarbageCollection.Gen0.Max} |");
            Console.WriteLine($"| GC Gen1 | {result.Gen1Collections} | {stats.GarbageCollection.Gen1.Min} - {stats.GarbageCollection.Gen1.Max} |");
            Console.WriteLine($"| GC Gen2 | {result.Gen2Collections} | {stats.GarbageCollection.Gen2.Min} - {stats.GarbageCollection.Gen2.Max} |");
        }
        else
        {
            Console.WriteLine("| Metric | Value |");
            Console.WriteLine("|--------|-------|");
            Console.WriteLine($"| Runtime | {result.RuntimeMs:F2}ms |");
            Console.WriteLine($"| Peak Memory | {result.PeakMemoryBytes / 1024.0 / 1024.0:F2}MB |");
            Console.WriteLine($"| GC Gen0/1/2 | {result.Gen0Collections}/{result.Gen1Collections}/{result.Gen2Collections} |");
        }

        Console.WriteLine();
        Console.WriteLine($"**Commit:** `{result.CommitHash}`");
        Console.WriteLine($"**Timestamp:** {result.Timestamp:yyyy-MM-dd HH:mm:ss} UTC");
    }

    public void ShowComparison(ComparisonReport report)
    {
        Console.WriteLine("## Performance Comparison");
        Console.WriteLine();
        Console.WriteLine($"**Current:** `{report.Current.CommitHash}` ({report.Current.Timestamp:yyyy-MM-dd HH:mm:ss})");
        Console.WriteLine($"**Previous:** `{report.Previous.CommitHash}` ({report.Previous.Timestamp:yyyy-MM-dd HH:mm:ss})");
        Console.WriteLine();

        bool hasRegressions = report.Metrics.Any(m => m.IsRegression);

        if (hasRegressions)
        {
            Console.WriteLine("> ⚠️ **Performance regressions detected**");
            Console.WriteLine();
        }

        Console.WriteLine("| Metric | Current | Previous | Change | Status |");
        Console.WriteLine("|--------|---------|----------|--------|--------|");

        foreach (var metric in report.Metrics)
        {
            var changeStr = $"{(metric.PercentageChange >= 0 ? "+" : "")}{metric.PercentageChange:F1}%";
            var status = metric.Direction switch
            {
                ChangeDirection.Degraded when metric.IsRegression => "⚠️ Regression",
                ChangeDirection.Degraded => "↑ Slower",
                ChangeDirection.Improved => "✓ Faster",
                _ => "→ Unchanged"
            };

            Console.WriteLine($"| {metric.Name} | {metric.CurrentValue:F2}{metric.Unit} | {metric.PreviousValue:F2}{metric.Unit} | {changeStr} | {status} |");
        }
    }

    public void ShowHistory(BenchmarkHistory history)
    {
        Console.WriteLine($"## Benchmark History: {history.ProgramName}");
        Console.WriteLine();
        Console.WriteLine($"**Total runs:** {history.Results.Count}");
        Console.WriteLine();

        if (history.Results.Count > 0)
        {
            Console.WriteLine("### Recent Results");
            Console.WriteLine();
            Console.WriteLine("| Date | Runtime | Memory | Commit |");
            Console.WriteLine("|------|---------|--------|--------|");

            foreach (var result in history.Results.TakeLast(10).Reverse())
            {
                var runsInfo = result.Statistics != null ? $" ({result.Statistics.TotalRuns}x)" : "";
                Console.WriteLine($"| {result.Timestamp:yyyy-MM-dd HH:mm:ss} | {result.RuntimeMs:F2}ms{runsInfo} | {result.PeakMemoryBytes / 1024.0 / 1024.0:F2}MB | `{result.CommitHash[..Math.Min(8, result.CommitHash.Length)]}` |");
            }
        }
    }
}
