namespace PerfReg.Models;

public record BenchmarkResult(
    string CommitHash,
    DateTime Timestamp,
    double RuntimeMs,
    long PeakMemoryBytes,
    int Gen0Collections,
    int Gen1Collections,
    int Gen2Collections
);
