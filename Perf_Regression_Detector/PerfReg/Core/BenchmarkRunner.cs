using System.Diagnostics;
using PerfReg.Models;
using PerfReg.Utilities;

namespace PerfReg.Core;

public class BenchmarkRunner : IBenchmarkRunner
{
    private readonly IMetricsCollector _metricsCollector;

    public BenchmarkRunner(IMetricsCollector metricsCollector)
    {
        _metricsCollector = metricsCollector;
    }

    public async Task<BenchmarkResult> RunAsync(string binary, string[] args)
    {
        return await RunAsync(binary, args, runs: 1, warmupRuns: 0);
    }

    public async Task<BenchmarkResult> RunAsync(string binary, string[] args, int runs, int warmupRuns)
    {
        var allMetrics = new List<MetricsData>();

        // Warmup runs
        if (warmupRuns > 0)
        {
            Console.WriteLine($"Running {warmupRuns} warmup run(s)...");
            for (int i = 0; i < warmupRuns; i++)
            {
                Console.Write($"  Warmup {i + 1}/{warmupRuns}...");
                await RunSingleBenchmark(binary, args);
                Console.WriteLine(" done");
            }
            Console.WriteLine();
        }

        // Actual benchmark runs
        if (runs > 1)
        {
            Console.WriteLine($"Running {runs} benchmark(s)...");
        }

        for (int i = 0; i < runs; i++)
        {
            if (runs > 1)
            {
                Console.Write($"  Run {i + 1}/{runs}...");
            }

            var metrics = await RunSingleBenchmark(binary, args);
            allMetrics.Add(metrics);

            if (runs > 1)
            {
                Console.WriteLine($" {metrics.RuntimeMs:F2}ms");
            }
        }

        if (runs > 1)
        {
            Console.WriteLine();
        }

        // Calculate statistics
        var avgMetrics = allMetrics[0]; // Use first run as baseline
        BenchmarkStatistics? statistics = null;

        if (runs > 1)
        {
            statistics = StatisticsCalculator.Calculate(allMetrics, warmupRuns);
            avgMetrics = new MetricsData(
                RuntimeMs: statistics.Runtime.Mean,
                PeakMemoryBytes: (long)statistics.Memory.Mean,
                Gen0Collections: (int)statistics.GarbageCollection.Gen0.Mean,
                Gen1Collections: (int)statistics.GarbageCollection.Gen1.Mean,
                Gen2Collections: (int)statistics.GarbageCollection.Gen2.Mean
            );
        }

        return new BenchmarkResult(
            CommitHash: GitHelper.GetCurrentCommitHash(),
            Timestamp: DateTime.UtcNow,
            RuntimeMs: avgMetrics.RuntimeMs,
            PeakMemoryBytes: avgMetrics.PeakMemoryBytes,
            Gen0Collections: avgMetrics.Gen0Collections,
            Gen1Collections: avgMetrics.Gen1Collections,
            Gen2Collections: avgMetrics.Gen2Collections,
            Statistics: statistics
        );
    }

    private async Task<MetricsData> RunSingleBenchmark(string binary, string[] args)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = binary,
            Arguments = string.Join(" ", args),
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        using var process = Process.Start(startInfo);
        if (process == null)
        {
            throw new InvalidOperationException("Failed to start process");
        }

        _metricsCollector.StartCollection(process);

        await process.WaitForExitAsync();
        return await _metricsCollector.StopCollectionAsync();
    }
}
