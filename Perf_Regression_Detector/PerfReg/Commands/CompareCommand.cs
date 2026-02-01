using PerfReg.Analysis;
using PerfReg.Reporting;
using PerfReg.Storage;

namespace PerfReg.Commands;

public class CompareCommand : ICommand
{
    private readonly IHistoryStorage _storage;
    private readonly IResultAnalyzer _analyzer;
    private readonly IReporter _reporter;

    public string Name => "compare";
    public string Description => "Compare last two benchmark runs";

    public CompareCommand(
        IHistoryStorage storage,
        IResultAnalyzer analyzer,
        IReporter reporter)
    {
        _storage = storage;
        _analyzer = analyzer;
        _reporter = reporter;
    }

    public Task<int> ExecuteAsync(string[] args)
    {
        var programs = _storage.ListPrograms().ToList();
        if (programs.Count == 0)
        {
            Console.WriteLine("No benchmark history found. Run a benchmark first.");
            return Task.FromResult(1);
        }

        var history = _storage.Load(programs[0]);

        if (history.Results.Count < 2)
        {
            Console.WriteLine("Need at least 2 benchmark runs to compare.");
            return Task.FromResult(1);
        }

        var comparison = _analyzer.Compare(history.Results[^1], history.Results[^2]);
        _reporter.ShowComparison(comparison);

        return Task.FromResult(0);
    }
}
