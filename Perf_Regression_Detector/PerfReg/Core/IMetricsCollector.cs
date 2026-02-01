using System.Diagnostics;

namespace PerfReg.Core;

public interface IMetricsCollector
{
    void StartCollection(Process process);
    Task<MetricsData> StopCollectionAsync();
}

public record MetricsData(
    double RuntimeMs,
    long PeakMemoryBytes,
    int Gen0Collections,
    int Gen1Collections,
    int Gen2Collections
);
