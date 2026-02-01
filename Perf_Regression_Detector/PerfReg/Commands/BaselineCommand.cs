using PerfReg.Reporting;
using PerfReg.Storage;
using PerfReg.Analysis;

namespace PerfReg.Commands;

public class BaselineCommand : ICommand
{
    private readonly IHistoryStorage _storage;
    private readonly IResultAnalyzer _analyzer;
    private readonly IReporter _reporter;

    public string Name => "baseline";
    public string Description => "Manage performance baselines";

    public BaselineCommand(IHistoryStorage storage, IResultAnalyzer analyzer, IReporter reporter)
    {
        _storage = storage;
        _analyzer = analyzer;
        _reporter = reporter;
    }

    public Task<int> ExecuteAsync(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: baseline <set|compare|show|clear> [program]");
            Console.WriteLine();
            Console.WriteLine("Commands:");
            Console.WriteLine("  set [program]     - Set current run as baseline");
            Console.WriteLine("  compare [program] - Compare latest run against baseline");
            Console.WriteLine("  show [program]    - Show current baseline");
            Console.WriteLine("  clear [program]   - Clear baseline");
            return Task.FromResult(1);
        }

        return args[0] switch
        {
            "set" => SetBaseline(args.Skip(1).ToArray()),
            "compare" => CompareBaseline(args.Skip(1).ToArray()),
            "show" => ShowBaseline(args.Skip(1).ToArray()),
            "clear" => ClearBaseline(args.Skip(1).ToArray()),
            _ => Task.FromResult(InvalidSubcommand(args[0]))
        };
    }

    private Task<int> SetBaseline(string[] args)
    {
        var programs = _storage.ListPrograms().ToList();

        if (programs.Count == 0)
        {
            Console.WriteLine("No benchmark history found. Run a benchmark first.");
            return Task.FromResult(1);
        }

        string programName = args.Length > 0 ? args[0] : programs[0];
        var history = _storage.Load(programName);

        if (history.Results.Count == 0)
        {
            Console.WriteLine($"No results found for {programName}");
            return Task.FromResult(1);
        }

        var baseline = history.Results[^1]; // Use latest result
        _storage.SaveBaseline(programName, baseline);

        Console.WriteLine($"✓ Baseline set for {programName}");
        Console.WriteLine($"  Commit: {baseline.CommitHash}");
        Console.WriteLine($"  Runtime: {baseline.RuntimeMs:F2}ms");
        Console.WriteLine($"  Memory: {baseline.PeakMemoryBytes / 1024.0 / 1024.0:F2}MB");

        return Task.FromResult(0);
    }

    private Task<int> CompareBaseline(string[] args)
    {
        var programs = _storage.ListPrograms().ToList();

        if (programs.Count == 0)
        {
            Console.WriteLine("No benchmark history found.");
            return Task.FromResult(1);
        }

        string programName = args.Length > 0 ? args[0] : programs[0];
        var baseline = _storage.LoadBaseline(programName);

        if (baseline == null)
        {
            Console.WriteLine($"No baseline set for {programName}. Use 'baseline set' first.");
            return Task.FromResult(1);
        }

        var history = _storage.Load(programName);
        if (history.Results.Count == 0)
        {
            Console.WriteLine($"No results found for {programName}");
            return Task.FromResult(1);
        }

        var current = history.Results[^1];
        var comparison = _analyzer.Compare(current, baseline);

        Console.WriteLine("=== Baseline Comparison ===");
        _reporter.ShowComparison(comparison);

        return Task.FromResult(0);
    }

    private Task<int> ShowBaseline(string[] args)
    {
        var programs = _storage.ListPrograms().ToList();

        if (programs.Count == 0)
        {
            Console.WriteLine("No benchmark history found.");
            return Task.FromResult(1);
        }

        if (args.Length > 0)
        {
            // Show specific program baseline
            var baseline = _storage.LoadBaseline(args[0]);
            if (baseline == null)
            {
                Console.WriteLine($"No baseline set for {args[0]}");
                return Task.FromResult(1);
            }

            ShowBaselineInfo(args[0], baseline);
        }
        else
        {
            // Show all baselines
            bool foundAny = false;
            foreach (var program in programs)
            {
                var baseline = _storage.LoadBaseline(program);
                if (baseline != null)
                {
                    ShowBaselineInfo(program, baseline);
                    foundAny = true;
                }
            }

            if (!foundAny)
            {
                Console.WriteLine("No baselines set. Use 'baseline set' to create one.");
            }
        }

        return Task.FromResult(0);
    }

    private Task<int> ClearBaseline(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: baseline clear <program>");
            return Task.FromResult(1);
        }

        _storage.ClearBaseline(args[0]);
        Console.WriteLine($"✓ Baseline cleared for {args[0]}");

        return Task.FromResult(0);
    }

    private void ShowBaselineInfo(string programName, Models.BenchmarkResult baseline)
    {
        Console.WriteLine($"\n{programName}:");
        Console.WriteLine($"  Commit: {baseline.CommitHash}");
        Console.WriteLine($"  Date: {baseline.Timestamp:yyyy-MM-dd HH:mm:ss}");
        Console.WriteLine($"  Runtime: {baseline.RuntimeMs:F2}ms");
        Console.WriteLine($"  Memory: {baseline.PeakMemoryBytes / 1024.0 / 1024.0:F2}MB");
        Console.WriteLine($"  GC: {baseline.Gen0Collections}/{baseline.Gen1Collections}/{baseline.Gen2Collections}");
    }

    private int InvalidSubcommand(string subcommand)
    {
        Console.WriteLine($"Error: Unknown subcommand '{subcommand}'");
        Console.WriteLine("Usage: baseline <set|compare|show|clear> [program]");
        return 1;
    }
}
