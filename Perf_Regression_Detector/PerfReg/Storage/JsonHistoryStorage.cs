using System.Text.Json;
using PerfReg.Models;

namespace PerfReg.Storage;

public class JsonHistoryStorage : IHistoryStorage
{
    public BenchmarkHistory Load(string programPath)
    {
        var programName = Path.GetFileNameWithoutExtension(programPath);
        var fileName = GetFileName(programName);

        if (!File.Exists(fileName))
        {
            return new BenchmarkHistory(programName, new List<BenchmarkResult>());
        }

        var json = File.ReadAllText(fileName);
        return JsonSerializer.Deserialize<BenchmarkHistory>(json)
            ?? new BenchmarkHistory(programName, new List<BenchmarkResult>());
    }

    public void Save(BenchmarkHistory history)
    {
        var fileName = GetFileName(history.ProgramName);
        var json = JsonSerializer.Serialize(history, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        File.WriteAllText(fileName, json);
    }

    public IEnumerable<string> ListPrograms()
    {
        var files = Directory.GetFiles(".", "*.benchmark.json");
        return files.Select(f => Path.GetFileNameWithoutExtension(f.Replace(".benchmark", "")));
    }

    public void Clear()
    {
        var files = Directory.GetFiles(".", "*.benchmark.json");
        foreach (var file in files)
        {
            File.Delete(file);
        }
    }

    private static string GetFileName(string programName)
    {
        return $"{programName}.benchmark.json";
    }
}
