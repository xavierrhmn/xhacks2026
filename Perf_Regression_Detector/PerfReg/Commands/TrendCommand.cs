using PerfReg.Analysis;
using PerfReg.Reporting;
using PerfReg.Storage;

namespace PerfReg.Commands;

public class TrendCommand : ICommand
{
    private readonly IHistoryStorage _storage;

    public string Name => "trend";
    public string Description => "Show performance trends with visualization";

    public TrendCommand(IHistoryStorage storage)
    {
        _storage = storage;
    }

    public Task<int> ExecuteAsync(string[] args)
    {
        var programs = _storage.ListPrograms().ToList();

        if (programs.Count == 0)
        {
            Console.WriteLine("No benchmark history found. Run a benchmark first.");
            return Task.FromResult(1);
        }

        // Parse arguments
        string? programName = args.Length > 0 ? args[0] : null;
        int windowSize = 10;

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--window" && i + 1 < args.Length)
            {
                int.TryParse(args[i + 1], out windowSize);
            }
        }

        if (programName == null)
        {
            programName = programs[0];
        }

        var history = _storage.Load(programName);

        if (history.Results.Count < 2)
        {
            Console.WriteLine($"Need at least 2 benchmark runs for trend analysis.");
            return Task.FromResult(1);
        }

        // Show trend analysis
        Console.WriteLine($"╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine($"║                    Trend Analysis: {programName,-26} ║");
        Console.WriteLine($"╚════════════════════════════════════════════════════════════════╝");
        Console.WriteLine();

        var analyzer = new TrendAnalyzer();
        var trendReport = analyzer.AnalyzeTrend(history.Results, windowSize);

        // Overall trend summary
        var directionSymbol = trendReport.OverallDirection switch
        {
            TrendDirection.Improving => "✓ Improving",
            TrendDirection.Degrading => "⚠️ Degrading",
            _ => "→ Stable"
        };

        Console.WriteLine($"Overall Trend: {directionSymbol}");
        Console.WriteLine($"Analysis Window: Last {Math.Min(windowSize, history.Results.Count)} runs");
        Console.WriteLine();

        // Per-metric trends
        Console.WriteLine("Metric Trends:");
        foreach (var metric in trendReport.Metrics)
        {
            var symbol = metric.Direction switch
            {
                TrendDirection.Improving => "✓",
                TrendDirection.Degrading => "⚠️",
                _ => "→"
            };

            var changeStr = $"{(metric.ChangePercent >= 0 ? "+" : "")}{metric.ChangePercent:F1}%";
            Console.WriteLine($"  {symbol} {metric.Name,-15}: {changeStr,10} over {Math.Min(windowSize, history.Results.Count)} runs");
        }

        // Visual charts
        var recentResults = history.Results.TakeLast(Math.Min(windowSize, history.Results.Count)).ToList();

        TerminalChartRenderer.RenderLineChart(recentResults, "Runtime");
        TerminalChartRenderer.RenderLineChart(recentResults, "Memory");

        // Sparklines
        Console.WriteLine("Quick View:");
        Console.Write("  Runtime:  ");
        TerminalChartRenderer.RenderSparkline(recentResults, "Runtime");
        Console.WriteLine($"  {recentResults[0].RuntimeMs:F1}ms → {recentResults[^1].RuntimeMs:F1}ms");

        Console.Write("  Memory:   ");
        TerminalChartRenderer.RenderSparkline(recentResults, "Memory");
        Console.WriteLine($"  {recentResults[0].PeakMemoryBytes / 1024.0 / 1024.0:F1}MB → {recentResults[^1].PeakMemoryBytes / 1024.0 / 1024.0:F1}MB");
        Console.WriteLine();

        // Histogram if multiple runs with statistics
        var runsWithStats = recentResults.Where(r => r.Statistics != null).ToList();
        if (runsWithStats.Count > 0)
        {
            var allRuntimes = new List<double>();
            foreach (var result in runsWithStats)
            {
                if (result.Statistics != null)
                {
                    // Add individual run times based on statistics
                    allRuntimes.Add(result.Statistics.Runtime.Min);
                    allRuntimes.Add(result.Statistics.Runtime.Median);
                    allRuntimes.Add(result.Statistics.Runtime.Max);
                }
            }

            if (allRuntimes.Count > 5)
            {
                TerminalChartRenderer.RenderHistogram(allRuntimes, "Runtime Distribution", 50);
            }
        }

        return Task.FromResult(0);
    }
}
