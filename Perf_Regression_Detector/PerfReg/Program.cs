using System.Diagnostics;
using System.Text.Json;

// === MODELS ===
record BenchmarkResult(
    string CommitHash,
    DateTime Timestamp,
    double RuntimeMs,
    long PeakMemoryBytes,
    int Gen0Collections,
    int Gen1Collections,
    int Gen2Collections
);

record BenchmarkHistory(
    string ProgramName,
    List<BenchmarkResult> Results
);

class Program
{
    const string HistoryFile = "benchmark_history.json";

    static async Task<int> Main(string[] args)
    {
        if (args.Length == 0)
        {
            ShowUsage();
            return 1;
        }

        return args[0] switch
        {
            "run" => await RunBenchmark(args.Skip(1).ToArray()),
            "compare" => ShowComparison(),
            "history" => ShowHistory(),
            "clear" => ClearHistory(),
            _ => InvalidCommand(args[0])
        };
    }

    static void ShowUsage()
    {
        Console.WriteLine("PerfReg - Performance Regression Detector");
        Console.WriteLine("\nUsage:");
        Console.WriteLine("  perfreg run <binary> [args...]  - Run benchmark and store results");
        Console.WriteLine("  perfreg compare                 - Compare last two runs");
        Console.WriteLine("  perfreg history                 - Show performance history");
        Console.WriteLine("  perfreg clear                   - Clear benchmark history");
    }

    static async Task<int> RunBenchmark(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Error: No binary specified");
            return 1;
        }

        string binary = args[0];
        string[] binaryArgs = args.Skip(1).ToArray();

        Console.WriteLine($"Running benchmark: {binary}");
        
        var gen0Before = GC.CollectionCount(0);
        var gen1Before = GC.CollectionCount(1);
        var gen2Before = GC.CollectionCount(2);

        var stopwatch = Stopwatch.StartNew();
        long peakMemory = 0;

        var startInfo = new ProcessStartInfo
        {
            FileName = binary,
            Arguments = string.Join(" ", binaryArgs),
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        using var process = Process.Start(startInfo);
        if (process == null)
        {
            Console.WriteLine("Error: Failed to start process");
            return 1;
        }

        var monitorTask = Task.Run(async () =>
        {
            while (!process.HasExited)
            {
                try
                {
                    process.Refresh();
                    peakMemory = Math.Max(peakMemory, process.WorkingSet64);
                }
                catch { }
                await Task.Delay(50);
            }
        });

        await process.WaitForExitAsync();
        await monitorTask;
        stopwatch.Stop();

        var result = new BenchmarkResult(
            CommitHash: GetGitCommitHash(),
            Timestamp: DateTime.UtcNow,
            RuntimeMs: stopwatch.Elapsed.TotalMilliseconds,
            PeakMemoryBytes: peakMemory,
            Gen0Collections: GC.CollectionCount(0) - gen0Before,
            Gen1Collections: GC.CollectionCount(1) - gen1Before,
            Gen2Collections: GC.CollectionCount(2) - gen2Before
        );

        var history = LoadHistory(binary);
        history.Results.Add(result);
        SaveHistory(history);

        Console.WriteLine($"\n✓ Benchmark complete!");
        Console.WriteLine($"  Runtime: {result.RuntimeMs:F2}ms");
        Console.WriteLine($"  Peak Memory: {result.PeakMemoryBytes / 1024.0 / 1024.0:F2}MB");
        Console.WriteLine($"  GC Gen0/1/2: {result.Gen0Collections}/{result.Gen1Collections}/{result.Gen2Collections}");

        if (history.Results.Count > 1)
        {
            Console.WriteLine();
            CompareResults(
                history.Results[^1], 
                history.Results[^2]
            );
        }

        return 0;
    }

    static int ShowComparison()
    {
        var files = Directory.GetFiles(".", "*.benchmark.json");
        if (files.Length == 0)
        {
            Console.WriteLine("No benchmark history found. Run a benchmark first.");
            return 1;
        }

        var history = LoadHistory(Path.GetFileNameWithoutExtension(files[0].Replace(".benchmark", "")));
        
        if (history.Results.Count < 2)
        {
            Console.WriteLine("Need at least 2 benchmark runs to compare.");
            return 1;
        }

        CompareResults(history.Results[^1], history.Results[^2]);
        return 0;
    }

    static void CompareResults(BenchmarkResult current, BenchmarkResult previous)
    {
        Console.WriteLine("=== Performance Comparison ===");
        Console.WriteLine($"Current:  {current.Timestamp:yyyy-MM-dd HH:mm:ss} ({current.CommitHash})");
        Console.WriteLine($"Previous: {previous.Timestamp:yyyy-MM-dd HH:mm:ss} ({previous.CommitHash})");
        Console.WriteLine();

        ShowMetricChange("Runtime", current.RuntimeMs, previous.RuntimeMs, "ms", lower: true);
        ShowMetricChange("Peak Memory", 
            current.PeakMemoryBytes / 1024.0 / 1024.0, 
            previous.PeakMemoryBytes / 1024.0 / 1024.0, 
            "MB", 
            lower: true);
        ShowMetricChange("GC Gen0", current.Gen0Collections, previous.Gen0Collections, "", lower: true);
        ShowMetricChange("GC Gen1", current.Gen1Collections, previous.Gen1Collections, "", lower: true);
        ShowMetricChange("GC Gen2", current.Gen2Collections, previous.Gen2Collections, "", lower: true);
    }

    static void ShowMetricChange(string name, double current, double previous, string unit, bool lower)
    {
        var change = ((current - previous) / previous) * 100.0;
        var changeStr = $"{(change >= 0 ? "+" : "")}{change:F1}%";
        
        var symbol = change switch
        {
            > 5 when lower => "⚠️ ",
            < -5 when !lower => "⚠️ ",
            > 1 => "↑",
            < -1 => "↓",
            _ => "→"
        };

        Console.WriteLine($"{name,-15}: {current:F2}{unit,-4} ({changeStr,8}) {symbol}");
    }

    static int ShowHistory()
    {
        var files = Directory.GetFiles(".", "*.benchmark.json");
        if (files.Length == 0)
        {
            Console.WriteLine("No benchmark history found.");
            return 1;
        }

        foreach (var file in files)
        {
            var programName = Path.GetFileNameWithoutExtension(file.Replace(".benchmark", ""));
            var history = LoadHistory(programName);
            
            Console.WriteLine($"\n=== {programName} ===");
            Console.WriteLine($"Total runs: {history.Results.Count}");
            
            if (history.Results.Count > 0)
            {
                Console.WriteLine("\nRecent runs:");
                foreach (var result in history.Results.TakeLast(5).Reverse())
                {
                    Console.WriteLine($"  {result.Timestamp:yyyy-MM-dd HH:mm:ss} - {result.RuntimeMs:F2}ms - {result.CommitHash[..Math.Min(8, result.CommitHash.Length)]}");
                }
            }
        }

        return 0;
    }

    static int ClearHistory()
    {
        var files = Directory.GetFiles(".", "*.benchmark.json");
        foreach (var file in files)
        {
            File.Delete(file);
        }
        Console.WriteLine($"Cleared {files.Length} history file(s)");
        return 0;
    }

    static BenchmarkHistory LoadHistory(string programName)
    {
        var fileName = $"{programName}.benchmark.json";
        
        if (!File.Exists(fileName))
        {
            return new BenchmarkHistory(programName, new List<BenchmarkResult>());
        }

        var json = File.ReadAllText(fileName);
        return JsonSerializer.Deserialize<BenchmarkHistory>(json) 
            ?? new BenchmarkHistory(programName, new List<BenchmarkResult>());
    }

    static void SaveHistory(BenchmarkHistory history)
    {
        var fileName = $"{history.ProgramName}.benchmark.json";
        var json = JsonSerializer.Serialize(history, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
        File.WriteAllText(fileName, json);
    }

    static string GetGitCommitHash()
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = "rev-parse --short HEAD",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process == null) return "unknown";
            
            var output = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();
            return string.IsNullOrEmpty(output) ? "unknown" : output;
        }
        catch
        {
            return "unknown";
        }
    }

    static int InvalidCommand(string command)
    {
        Console.WriteLine($"Error: Unknown command '{command}'");
        ShowUsage();
        return 1;
    }
}