using PerfReg.Analysis;
using PerfReg.Commands;
using PerfReg.Configuration;
using PerfReg.Core;
using PerfReg.Reporting;
using PerfReg.Storage;

namespace PerfReg;

class Program
{
    static async Task<int> Main(string[] args)
    {
        // Enable UTF-8 output for special characters (sparklines, box drawing)
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        if (args.Length == 0)
        {
            HelpMenu.ShowUsage();
            return 1;
        }

        // Load configuration
        var config = ConfigLoader.Load();

        // Initialize dependencies
        var storage = new JsonHistoryStorage();
        var metricsCollector = new ProcessMetricsCollector();
        var runner = new BenchmarkRunner(metricsCollector);
        var analyzer = new ComparisonAnalyzer();
        var reporter = new ConsoleReporter();

        // Create commands
        var commands = new Dictionary<string, ICommand>
        {
            ["run"] = new RunCommand(runner, storage, analyzer, reporter, config),
            ["compare"] = new CompareCommand(storage, analyzer, reporter),
            ["history"] = new HistoryCommand(storage, reporter),
            ["clear"] = new ClearCommand(storage),
            ["config"] = new ConfigCommand(),
            ["baseline"] = new BaselineCommand(storage, analyzer, reporter),
            ["export"] = new ExportCommand(storage),
            ["trend"] = new TrendCommand(storage),
            ["compare-historical"] = new CompareHistoricalCommand(storage, analyzer, reporter)
        };

        var commandName = args[0];
        var commandArgs = args.Skip(1).ToArray();

        return commandName switch
        {
            "help" or "--help" or "-h" => ShowHelp(),
            "version" or "--version" or "-v" => ShowVersion(),
            _ when commands.ContainsKey(commandName) => await commands[commandName].ExecuteAsync(commandArgs),
            _ => InvalidCommand(commandName)
        };
    }

    static int ShowHelp()
    {
        HelpMenu.ShowUsage();
        return 0;
    }

    static int ShowVersion()
    {
        HelpMenu.ShowVersion();
        return 0;
    }

    static int InvalidCommand(string command)
    {
        Console.WriteLine($"Error: Unknown command '{command}'");
        Console.WriteLine();
        HelpMenu.ShowUsage();
        return 1;
    }
}
