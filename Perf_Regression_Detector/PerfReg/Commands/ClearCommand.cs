using PerfReg.Storage;

namespace PerfReg.Commands;

public class ClearCommand : ICommand
{
    private readonly IHistoryStorage _storage;

    public string Name => "clear";
    public string Description => "Clear all benchmark history";

    public ClearCommand(IHistoryStorage storage)
    {
        _storage = storage;
    }

    public Task<int> ExecuteAsync(string[] args)
    {
        var count = _storage.ListPrograms().Count();
        _storage.Clear();
        Console.WriteLine($"Cleared {count} history file(s)");
        return Task.FromResult(0);
    }
}
