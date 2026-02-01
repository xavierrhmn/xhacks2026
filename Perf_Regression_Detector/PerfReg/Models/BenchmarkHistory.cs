namespace PerfReg.Models;

public record BenchmarkHistory(
    string ProgramName,
    List<BenchmarkResult> Results
);
