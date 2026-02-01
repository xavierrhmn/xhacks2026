using PerfReg.Core;
using PerfReg.Models;

namespace PerfReg.Utilities;

public static class StatisticsCalculator
{
    public static BenchmarkStatistics Calculate(List<MetricsData> runs, int warmupRuns)
    {
        var runtimes = runs.Select(r => r.RuntimeMs).ToList();
        var memories = runs.Select(r => r.PeakMemoryBytes).ToList();
        var gen0s = runs.Select(r => r.Gen0Collections).ToList();
        var gen1s = runs.Select(r => r.Gen1Collections).ToList();
        var gen2s = runs.Select(r => r.Gen2Collections).ToList();

        return new BenchmarkStatistics(
            TotalRuns: runs.Count,
            WarmupRuns: warmupRuns,
            Runtime: new RuntimeStats(
                Mean: runtimes.Average(),
                Median: Median(runtimes),
                StdDev: StandardDeviation(runtimes),
                Min: runtimes.Min(),
                Max: runtimes.Max()
            ),
            Memory: new MemoryStats(
                Mean: memories.Average(),
                Median: Median(memories.Select(m => (double)m).ToList()),
                StdDev: StandardDeviation(memories.Select(m => (double)m).ToList()),
                Min: memories.Min(),
                Max: memories.Max()
            ),
            GarbageCollection: new GcStats(
                Gen0: new GcGenStats(
                    Mean: gen0s.Average(),
                    Median: Median(gen0s.Select(g => (double)g).ToList()),
                    Min: gen0s.Min(),
                    Max: gen0s.Max()
                ),
                Gen1: new GcGenStats(
                    Mean: gen1s.Average(),
                    Median: Median(gen1s.Select(g => (double)g).ToList()),
                    Min: gen1s.Min(),
                    Max: gen1s.Max()
                ),
                Gen2: new GcGenStats(
                    Mean: gen2s.Average(),
                    Median: Median(gen2s.Select(g => (double)g).ToList()),
                    Min: gen2s.Min(),
                    Max: gen2s.Max()
                )
            )
        );
    }

    private static double Median(List<double> values)
    {
        var sorted = values.OrderBy(v => v).ToList();
        int count = sorted.Count;

        if (count == 0) return 0;
        if (count == 1) return sorted[0];

        if (count % 2 == 0)
        {
            return (sorted[count / 2 - 1] + sorted[count / 2]) / 2.0;
        }
        else
        {
            return sorted[count / 2];
        }
    }

    private static double StandardDeviation(List<double> values)
    {
        if (values.Count <= 1) return 0;

        double mean = values.Average();
        double sumSquaredDiff = values.Sum(v => Math.Pow(v - mean, 2));
        return Math.Sqrt(sumSquaredDiff / (values.Count - 1));
    }
}
