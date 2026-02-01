namespace PerfReg
{
    public static class HelpMenu
    {
        public static void ShowUsage()
        {
            Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║           PerfReg - Performance Regression Detector            ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
            Console.WriteLine();
            Console.WriteLine("USAGE:");
            Console.WriteLine("  perfreg <command> [options]");
            Console.WriteLine();
            Console.WriteLine("COMMANDS:");
            Console.WriteLine();
            ShowCommand("run", "<binary> [args...] [options]", "Run benchmark and store results");
            ShowCommand("compare", "", "Compare last two benchmark runs");
            ShowCommand("history", "", "Show performance history for all programs");
            ShowCommand("trend", "[program] [--window N]", "Show performance trends with charts");
            ShowCommand("baseline", "<set|compare|show|clear>", "Manage performance baselines");
            ShowCommand("export", "[program]", "Export benchmark data as JSON");
            ShowCommand("compare-historical", "<commit>", "Compare against specific commit");
            ShowCommand("clear", "", "Clear all benchmark history");
            ShowCommand("config", "", "Create default .perfreg.json config file");
            ShowCommand("help", "", "Show this help menu");
            Console.WriteLine();
            Console.WriteLine("RUN OPTIONS:");
            Console.WriteLine("  --runs N                         Run benchmark N times (default: 1)");
            Console.WriteLine("  --warmup N                       Run N warmup iterations (default: 0)");
            Console.WriteLine("  --fail-on-regression             Exit with code 1 if regression detected");
            Console.WriteLine();
            Console.WriteLine("EXAMPLES:");
            Console.WriteLine();
            ShowExample(
                "Run a benchmark",
                "perfreg run MyApp.exe"
            );
            ShowExample(
                "Run with arguments",
                "perfreg run MyApp.exe arg1 arg2"
            );
            ShowExample(
                "Compare results",
                "perfreg compare"
            );
            ShowExample(
                "View history",
                "perfreg history"
            );
            ShowExample(
                "Multiple runs with statistics",
                "perfreg run MyApp.exe --runs 5"
            );
            ShowExample(
                "With warmup runs",
                "perfreg run MyApp.exe --runs 5 --warmup 2"
            );
            ShowExample(
                "Create config file",
                "perfreg config"
            );
            ShowExample(
                "Set baseline",
                "perfreg baseline set"
            );
            ShowExample(
                "Compare against baseline",
                "perfreg baseline compare"
            );
            ShowExample(
                "Export as JSON",
                "perfreg export > results.json"
            );
            ShowExample(
                "Fail CI build on regression",
                "perfreg run MyApp.exe --fail-on-regression"
            );
            ShowExample(
                "Show trend analysis with charts",
                "perfreg trend"
            );
            ShowExample(
                "Compare against specific commit",
                "perfreg compare-historical abc1234"
            );
            Console.WriteLine();
            Console.WriteLine("TRACKED METRICS:");
            Console.WriteLine("  • Runtime (milliseconds)");
            Console.WriteLine("  • Peak Memory Usage (MB)");
            Console.WriteLine("  • GC Collections (Gen 0, 1, 2)");
            Console.WriteLine("  • Git Commit Hash (if available)");
            Console.WriteLine();
            Console.WriteLine("OUTPUT:");
            Console.WriteLine("  Results are stored in <program-name>.benchmark.json");
            Console.WriteLine("  Warnings (⚠️) appear when metrics degrade by >5%");
            Console.WriteLine();
        }

        public static void ShowVersion()
        {
            Console.WriteLine("PerfReg v3.0.0");
            Console.WriteLine("Performance Regression Detection Tool for .NET");
            Console.WriteLine("Sprint 3: Advanced Analysis & Visualization");
            Console.WriteLine();
        }

        private static void ShowCommand(string command, string arguments, string description)
        {
            var cmdText = $"  {command}";
            if (!string.IsNullOrEmpty(arguments))
            {
                cmdText += $" {arguments}";
            }
            
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(cmdText.PadRight(35));
            Console.ResetColor();
            Console.WriteLine(description);
        }

        private static void ShowExample(string description, string command)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"  {description}:");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($"    {command}");
            Console.ResetColor();
            Console.WriteLine();
        }
    }
}