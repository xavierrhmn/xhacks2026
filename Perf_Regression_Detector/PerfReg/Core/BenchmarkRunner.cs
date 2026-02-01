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
        var metrics = await _metricsCollector.StopCollectionAsync();

        return new BenchmarkResult(
            CommitHash: GitHelper.GetCurrentCommitHash(),
            Timestamp: DateTime.UtcNow,
            RuntimeMs: metrics.RuntimeMs,
            PeakMemoryBytes: metrics.PeakMemoryBytes,
            Gen0Collections: metrics.Gen0Collections,
            Gen1Collections: metrics.Gen1Collections,
            Gen2Collections: metrics.Gen2Collections
        );
    }
}
