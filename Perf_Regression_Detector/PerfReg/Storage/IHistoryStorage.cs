using PerfReg.Models;

namespace PerfReg.Storage;

public interface IHistoryStorage
{
    BenchmarkHistory Load(string programName);
    void Save(BenchmarkHistory history);
    IEnumerable<string> ListPrograms();
    void Clear();
}
