using System.Text.Json;

namespace PerfReg.Configuration;

public static class ConfigLoader
{
    private const string ConfigFileName = ".perfreg.json";

    public static PerfRegConfig Load()
    {
        if (!File.Exists(ConfigFileName))
        {
            return new PerfRegConfig();
        }

        try
        {
            var json = File.ReadAllText(ConfigFileName);
            return JsonSerializer.Deserialize<PerfRegConfig>(json) ?? new PerfRegConfig();
        }
        catch
        {
            Console.WriteLine($"Warning: Could not load {ConfigFileName}, using defaults");
            return new PerfRegConfig();
        }
    }

    public static void SaveDefault()
    {
        var config = new PerfRegConfig();
        var json = JsonSerializer.Serialize(config, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        File.WriteAllText(ConfigFileName, json);
        Console.WriteLine($"Created default configuration file: {ConfigFileName}");
    }
}
