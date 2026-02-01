using System.Text.Json;
using PerfReg.Analysis;
using PerfReg.Models;

namespace PerfReg.Reporting;

public class JsonReporter : IReporter
{
    public void ShowResult(BenchmarkResult result)
    {
        var output = new
        {
            result.CommitHash,
            result.Timestamp,
            result.RuntimeMs,
            result.PeakMemoryBytes,
            result.Gen0Collections,
            result.Gen1Collections,
            result.Gen2Collections,
            result.Statistics
        };

        Console.WriteLine(JsonSerializer.Serialize(output, new JsonSerializerOptions
        {
            WriteIndented = true
        }));
    }

    public void ShowComparison(ComparisonReport report)
    {
        var output = new
        {
            Current = new
            {
                report.Current.CommitHash,
                report.Current.Timestamp,
                report.Current.RuntimeMs,
                report.Current.PeakMemoryBytes,
                report.Current.Gen0Collections,
                report.Current.Gen1Collections,
                report.Current.Gen2Collections
            },
            Previous = new
            {
                report.Previous.CommitHash,
                report.Previous.Timestamp,
                report.Previous.RuntimeMs,
                report.Previous.PeakMemoryBytes,
                report.Previous.Gen0Collections,
                report.Previous.Gen1Collections,
                report.Previous.Gen2Collections
            },
            Metrics = report.Metrics.Select(m => new
            {
                m.Name,
                m.CurrentValue,
                m.PreviousValue,
                m.PercentageChange,
                m.Unit,
                m.IsRegression,
                Direction = m.Direction.ToString()
            })
        };

        Console.WriteLine(JsonSerializer.Serialize(output, new JsonSerializerOptions
        {
            WriteIndented = true
        }));
    }

    public void ShowHistory(BenchmarkHistory history)
    {
        Console.WriteLine(JsonSerializer.Serialize(history, new JsonSerializerOptions
        {
            WriteIndented = true
        }));
    }
}
