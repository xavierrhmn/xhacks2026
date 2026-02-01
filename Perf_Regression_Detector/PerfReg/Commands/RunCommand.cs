using PerfReg.Analysis;
using PerfReg.Configuration;
using PerfReg.Core;
using PerfReg.Reporting;
using PerfReg.Storage;

namespace PerfReg.Commands;

public class RunCommand : ICommand
{
    private readonly IBenchmarkRunner _runner;
    private readonly IHistoryStorage _storage;
    private readonly IReporter _reporter;
    private readonly PerfRegConfig _config;

    public string Name => "run";
    public string Description => "Run benchmark and store results";

    public RunCommand(
        IBenchmarkRunner runner,
        IHistoryStorage storage,
        IResultAnalyzer analyzer,  // Keep for interface compatibility, but we create our own
        IReporter reporter,
        PerfRegConfig config)
    {
        _runner = runner;
        _storage = storage;
        _reporter = reporter;
        _config = config;
    }

    public async Task<int> ExecuteAsync(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Error: No binary specified");
            Console.WriteLine();
            HelpMenu.ShowUsage();
            return 1;
        }

        // Parse arguments
        var (binary, binaryArgs, runs, warmupRuns, failOnRegression, threshold) = ParseArguments(args);

        Console.WriteLine($"Running benchmark: {binary}");
        if (runs > 1 || warmupRuns > 0)
        {
            Console.WriteLine($"Configuration: {runs} run(s), {warmupRuns} warmup(s)");
            Console.WriteLine();
        }

        try
        {
            var result = await _runner.RunAsync(binary, binaryArgs, runs, warmupRuns);

            var history = _storage.Load(binary);
            history.Results.Add(result);
            _storage.Save(history);

            _reporter.ShowResult(result);

            bool hasRegression = false;

            if (history.Results.Count > 1)
            {
                Console.WriteLine();
                // Create analyzer with the specified threshold
                var analyzer = new ComparisonAnalyzer(threshold);
                var comparison = analyzer.Compare(
                    history.Results[^1],
                    history.Results[^2]
                );
                _reporter.ShowComparison(comparison);

                // Check for regressions
                hasRegression = comparison.Metrics.Any(m => m.IsRegression);
            }

            // Return non-zero exit code if regression detected and flag is set
            if (hasRegression && failOnRegression)
            {
                Console.WriteLine();
                Console.WriteLine("❌ Build failed: Performance regression detected");
                return 1;
            }

            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }

    private (string binary, string[] binaryArgs, int runs, int warmupRuns, bool failOnRegression, double threshold) ParseArguments(string[] args)
    {
        string binary = "";
        var binaryArgs = new List<string>();
        int runs = _config.DefaultRuns;
        int warmupRuns = _config.DefaultWarmupRuns;
        bool failOnRegression = _config.FailOnRegression;
        double threshold = 5.0;  // Default 5%

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--runs" && i + 1 < args.Length)
            {
                if (int.TryParse(args[i + 1], out int r) && r > 0)
                {
                    runs = r;
                    i++; // Skip next arg
                }
            }
            else if (args[i] == "--warmup" && i + 1 < args.Length)
            {
                if (int.TryParse(args[i + 1], out int w) && w >= 0)
                {
                    warmupRuns = w;
                    i++; // Skip next arg
                }
            }
            else if (args[i] == "--threshold" && i + 1 < args.Length)
            {
                if (double.TryParse(args[i + 1], out double t) && t > 0)
                {
                    threshold = t;
                    i++; // Skip next arg
                }
            }
            else if (args[i] == "--fail-on-regression")
            {
                failOnRegression = true;
            }
            else if (string.IsNullOrEmpty(binary))
            {
                binary = args[i];
            }
            else
            {
                binaryArgs.Add(args[i]);
            }
        }

        return (binary, binaryArgs.ToArray(), runs, warmupRuns, failOnRegression, threshold);
    }
}
