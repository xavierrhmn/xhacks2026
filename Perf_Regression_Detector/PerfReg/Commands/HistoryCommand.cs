using PerfReg.Reporting;
using PerfReg.Storage;

namespace PerfReg.Commands;

public class HistoryCommand : ICommand
{
    private readonly IHistoryStorage _storage;
    private readonly IReporter _reporter;

    public string Name => "history";
    public string Description => "Show performance history for all programs";

    public HistoryCommand(IHistoryStorage storage, IReporter reporter)
    {
        _storage = storage;
        _reporter = reporter;
    }

    public Task<int> ExecuteAsync(string[] args)
    {
        var programs = _storage.ListPrograms().ToList();
        if (programs.Count == 0)
        {
            Console.WriteLine("No benchmark history found.");
            return Task.FromResult(1);
        }

        foreach (var programName in programs)
        {
            var history = _storage.Load(programName);
            _reporter.ShowHistory(history);
        }

        return Task.FromResult(0);
    }
}
