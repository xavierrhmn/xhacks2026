using System.Text.Json;
using PerfReg.Storage;

namespace PerfReg.Commands;

public class ExportCommand : ICommand
{
    private readonly IHistoryStorage _storage;

    public string Name => "export";
    public string Description => "Export benchmark data as JSON";

    public ExportCommand(IHistoryStorage storage)
    {
        _storage = storage;
    }

    public Task<int> ExecuteAsync(string[] args)
    {
        var programs = _storage.ListPrograms().ToList();

        if (programs.Count == 0)
        {
            Console.WriteLine("No benchmark history found.");
            return Task.FromResult(1);
        }

        if (args.Length > 0)
        {
            // Export specific program
            var programName = args[0];
            var history = _storage.Load(programName);

            if (history.Results.Count == 0)
            {
                Console.WriteLine($"No results found for {programName}");
                return Task.FromResult(1);
            }

            ExportHistory(history);
        }
        else
        {
            // Export all programs
            var allData = programs.Select(p => _storage.Load(p)).ToList();
            ExportAllHistories(allData);
        }

        return Task.FromResult(0);
    }

    private void ExportHistory(Models.BenchmarkHistory history)
    {
        var json = JsonSerializer.Serialize(history, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        Console.WriteLine(json);
    }

    private void ExportAllHistories(List<Models.BenchmarkHistory> histories)
    {
        var json = JsonSerializer.Serialize(histories, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        Console.WriteLine(json);
    }
}
