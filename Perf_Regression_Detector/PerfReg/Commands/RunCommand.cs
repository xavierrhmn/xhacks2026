using PerfReg.Analysis;
using PerfReg.Core;
using PerfReg.Reporting;
using PerfReg.Storage;

namespace PerfReg.Commands;

public class RunCommand : ICommand
{
    private readonly IBenchmarkRunner _runner;
    private readonly IHistoryStorage _storage;
    private readonly IResultAnalyzer _analyzer;
    private readonly IReporter _reporter;

    public string Name => "run";
    public string Description => "Run benchmark and store results";

    public RunCommand(
        IBenchmarkRunner runner,
        IHistoryStorage storage,
        IResultAnalyzer analyzer,
        IReporter reporter)
    {
        _runner = runner;
        _storage = storage;
        _analyzer = analyzer;
        _reporter = reporter;
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

        string binary = args[0];
        string[] binaryArgs = args.Skip(1).ToArray();

        Console.WriteLine($"Running benchmark: {binary}");

        try
        {
            var result = await _runner.RunAsync(binary, binaryArgs);

            var history = _storage.Load(binary);
            history.Results.Add(result);
            _storage.Save(history);

            _reporter.ShowResult(result);

            if (history.Results.Count > 1)
            {
                Console.WriteLine();
                var comparison = _analyzer.Compare(
                    history.Results[^1],
                    history.Results[^2]
                );
                _reporter.ShowComparison(comparison);
            }

            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }
}
