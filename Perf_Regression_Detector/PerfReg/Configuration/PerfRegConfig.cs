namespace PerfReg.Configuration;

public class PerfRegConfig
{
    public int DefaultRuns { get; set; } = 1;
    public int DefaultWarmupRuns { get; set; } = 0;
    public bool FailOnRegression { get; set; } = false;
    public ThresholdsConfig Thresholds { get; set; } = new();
}

public class ThresholdsConfig
{
    public double RuntimePercent { get; set; } = 5.0;
    public double MemoryPercent { get; set; } = 5.0;
    public double GcGen0Percent { get; set; } = 5.0;
    public double GcGen1Percent { get; set; } = 5.0;
    public double GcGen2Percent { get; set; } = 5.0;
}
