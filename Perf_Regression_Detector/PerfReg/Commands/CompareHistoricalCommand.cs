using PerfReg.Analysis;
using PerfReg.Reporting;
using PerfReg.Storage;

namespace PerfReg.Commands;

public class CompareHistoricalCommand : ICommand
{
    private readonly IHistoryStorage _storage;
    private readonly IResultAnalyzer _analyzer;
    private readonly IReporter _reporter;

    public string Name => "compare-historical";
    public string Description => "Compare benchmark results across commits";

    public CompareHistoricalCommand(IHistoryStorage storage, IResultAnalyzer analyzer, IReporter reporter)
    {
        _storage = storage;
        _analyzer = analyzer;
        _reporter = reporter;
    }

    public Task<int> ExecuteAsync(string[] args)
    {
        if (args.Length < 1)
        {
            Console.WriteLine("Usage: compare-historical <commit-hash> [program]");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  compare-historical abc1234");
            Console.WriteLine("  compare-historical abc1234 MyApp");
            return Task.FromResult(1);
        }

        var targetCommit = args[0];
        var programs = _storage.ListPrograms().ToList();

        if (programs.Count == 0)
        {
            Console.WriteLine("No benchmark history found.");
            return Task.FromResult(1);
        }

        string programName = args.Length > 1 ? args[1] : programs[0];
        var history = _storage.Load(programName);

        if (history.Results.Count == 0)
        {
            Console.WriteLine($"No results found for {programName}");
            return Task.FromResult(1);
        }

        // Find result with matching commit
        var targetResult = history.Results.FirstOrDefault(r =>
            r.CommitHash.StartsWith(targetCommit, StringComparison.OrdinalIgnoreCase));

        if (targetResult == null)
        {
            Console.WriteLine($"No benchmark found for commit {targetCommit}");
            Console.WriteLine();
            Console.WriteLine("Available commits:");
            foreach (var result in history.Results.TakeLast(10).Reverse())
            {
                Console.WriteLine($"  {result.CommitHash} - {result.Timestamp:yyyy-MM-dd HH:mm:ss}");
            }
            return Task.FromResult(1);
        }

        var currentResult = history.Results[^1];

        if (targetResult == currentResult)
        {
            Console.WriteLine("Target commit is the same as the latest run.");
            return Task.FromResult(1);
        }

        Console.WriteLine($"═══ Historical Comparison: {programName} ═══");
        Console.WriteLine();
        Console.WriteLine($"Comparing:");
        Console.WriteLine($"  Current:  {currentResult.CommitHash} ({currentResult.Timestamp:yyyy-MM-dd HH:mm:ss})");
        Console.WriteLine($"  Target:   {targetResult.CommitHash} ({targetResult.Timestamp:yyyy-MM-dd HH:mm:ss})");
        Console.WriteLine();

        var comparison = _analyzer.Compare(currentResult, targetResult);
        _reporter.ShowComparison(comparison);

        // Show trend between commits
        var resultsBetween = history.Results
            .SkipWhile(r => r != targetResult)
            .TakeWhile(r => r != currentResult)
            .Append(currentResult)
            .ToList();

        if (resultsBetween.Count > 2)
        {
            Console.WriteLine();
            Console.WriteLine($"═══ Trend Over {resultsBetween.Count} Commits ═══");
            Console.WriteLine();

            var analyzer = new TrendAnalyzer();
            var trendReport = analyzer.AnalyzeTrend(resultsBetween, resultsBetween.Count);

            foreach (var metric in trendReport.Metrics)
            {
                var symbol = metric.Direction switch
                {
                    TrendDirection.Improving => "✓",
                    TrendDirection.Degrading => "⚠️",
                    _ => "→"
                };

                Console.WriteLine($"  {symbol} {metric.Name}: {(metric.ChangePercent >= 0 ? "+" : "")}{metric.ChangePercent:F1}%");
            }

            Console.WriteLine();
            Console.Write("  Runtime trend: ");
            TerminalChartRenderer.RenderSparkline(resultsBetween, "Runtime");
            Console.WriteLine();
        }

        return Task.FromResult(0);
    }
}
