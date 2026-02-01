namespace PerfReg.Models;

public record BenchmarkResult(
    string CommitHash,
    DateTime Timestamp,
    double RuntimeMs,
    long PeakMemoryBytes,
    int Gen0Collections,
    int Gen1Collections,
    int Gen2Collections,
    BenchmarkStatistics? Statistics = null
);

public record BenchmarkStatistics(
    int TotalRuns,
    int WarmupRuns,
    RuntimeStats Runtime,
    MemoryStats Memory,
    GcStats GarbageCollection
);

public record RuntimeStats(
    double Mean,
    double Median,
    double StdDev,
    double Min,
    double Max
);

public record MemoryStats(
    double Mean,
    double Median,
    double StdDev,
    long Min,
    long Max
);

public record GcStats(
    GcGenStats Gen0,
    GcGenStats Gen1,
    GcGenStats Gen2
);

public record GcGenStats(
    double Mean,
    double Median,
    int Min,
    int Max
);
