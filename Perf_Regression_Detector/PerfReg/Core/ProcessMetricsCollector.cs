using System.Diagnostics;

namespace PerfReg.Core;

public class ProcessMetricsCollector : IMetricsCollector
{
    private Process? _process;
    private Stopwatch? _stopwatch;
    private long _peakMemory;
    private int _gen0Before;
    private int _gen1Before;
    private int _gen2Before;
    private Task? _monitorTask;

    public void StartCollection(Process process)
    {
        _process = process;
        _peakMemory = 0;

        _gen0Before = GC.CollectionCount(0);
        _gen1Before = GC.CollectionCount(1);
        _gen2Before = GC.CollectionCount(2);

        _stopwatch = Stopwatch.StartNew();

        _monitorTask = Task.Run(async () =>
        {
            while (!_process.HasExited)
            {
                try
                {
                    _process.Refresh();
                    _peakMemory = Math.Max(_peakMemory, _process.WorkingSet64);
                }
                catch { }
                await Task.Delay(50);
            }
        });
    }

    public async Task<MetricsData> StopCollectionAsync()
    {
        if (_stopwatch == null || _monitorTask == null)
        {
            throw new InvalidOperationException("Collection not started");
        }

        await _monitorTask;
        _stopwatch.Stop();

        return new MetricsData(
            RuntimeMs: _stopwatch.Elapsed.TotalMilliseconds,
            PeakMemoryBytes: _peakMemory,
            Gen0Collections: GC.CollectionCount(0) - _gen0Before,
            Gen1Collections: GC.CollectionCount(1) - _gen1Before,
            Gen2Collections: GC.CollectionCount(2) - _gen2Before
        );
    }
}
