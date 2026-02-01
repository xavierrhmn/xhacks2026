namespace DemoApp;

class Program
{
    static void Main(string[] args)
    {
        // Configurable demo scenarios
        string scenario = args.Length > 0 ? args[0] : "fast";

        switch (scenario)
        {
            case "fast":
                RunFastScenario();
                break;
            case "slow":
                RunSlowScenario();
                break;
            case "memory":
                RunMemoryIntensiveScenario();
                break;
            case "variable":
                RunVariableScenario();
                break;
            default:
                Console.WriteLine("Usage: DemoApp [fast|slow|memory|variable]");
                break;
        }
    }

    static void RunFastScenario()
    {
        // Lightweight computation - ~50ms
        Console.WriteLine("Running fast scenario...");
        var sum = 0;
        for (int i = 0; i < 1_000_000; i++)
        {
            sum += i;
        }
        Thread.Sleep(50);
        Console.WriteLine($"Fast scenario complete. Sum: {sum}");
    }

    static void RunSlowScenario()
    {
        // Heavier computation - ~300ms
        Console.WriteLine("Running slow scenario...");
        var sum = 0.0;
        for (int i = 0; i < 10_000_000; i++)
        {
            sum += Math.Sqrt(i);
        }
        Thread.Sleep(100);
        Console.WriteLine($"Slow scenario complete. Sum: {sum:F2}");
    }

    static void RunMemoryIntensiveScenario()
    {
        // Allocate memory - creates GC pressure
        Console.WriteLine("Running memory-intensive scenario...");
        var lists = new List<byte[]>();

        // Allocate ~50MB
        for (int i = 0; i < 50; i++)
        {
            lists.Add(new byte[1024 * 1024]); // 1MB each
        }

        Thread.Sleep(100);

        // Process data
        var total = 0L;
        foreach (var arr in lists)
        {
            total += arr.Length;
        }

        Console.WriteLine($"Memory scenario complete. Allocated: {total / 1024 / 1024}MB");
    }

    static void RunVariableScenario()
    {
        // Variable performance - introduces variance for percentile demo
        Console.WriteLine("Running variable scenario...");
        var random = new Random();
        var delay = random.Next(50, 300);

        var sum = 0.0;
        var iterations = random.Next(1_000_000, 5_000_000);
        for (int i = 0; i < iterations; i++)
        {
            sum += Math.Sqrt(i);
        }

        Thread.Sleep(delay);
        Console.WriteLine($"Variable scenario complete. Delay: {delay}ms");
    }
}
