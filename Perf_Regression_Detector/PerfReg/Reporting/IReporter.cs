using PerfReg.Analysis;
using PerfReg.Models;

namespace PerfReg.Reporting;

public interface IReporter
{
    void ShowResult(BenchmarkResult result);
    void ShowComparison(ComparisonReport report);
    void ShowHistory(BenchmarkHistory history);
}
