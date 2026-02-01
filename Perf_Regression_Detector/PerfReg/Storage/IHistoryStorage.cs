using PerfReg.Models;

namespace PerfReg.Storage;

public interface IHistoryStorage
{
    BenchmarkHistory Load(string programName);
    void Save(BenchmarkHistory history);
    IEnumerable<string> ListPrograms();
    void Clear();

    // Baseline management
    BenchmarkResult? LoadBaseline(string programName);
    void SaveBaseline(string programName, BenchmarkResult baseline);
    void ClearBaseline(string programName);
}
