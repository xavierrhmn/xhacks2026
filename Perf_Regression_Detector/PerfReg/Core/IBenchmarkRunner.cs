using PerfReg.Models;

namespace PerfReg.Core;

public interface IBenchmarkRunner
{
    Task<BenchmarkResult> RunAsync(string binary, string[] args);
}
