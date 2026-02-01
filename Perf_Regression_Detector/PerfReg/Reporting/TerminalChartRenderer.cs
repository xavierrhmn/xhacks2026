using PerfReg.Models;

namespace PerfReg.Reporting;

public static class TerminalChartRenderer
{
    public static void RenderLineChart(List<BenchmarkResult> results, string metricName, int width = 60, int height = 10)
    {
        if (results.Count < 2)
        {
            Console.WriteLine("Not enough data points for chart");
            return;
        }

        var values = metricName switch
        {
            "Runtime" => results.Select(r => r.RuntimeMs).ToList(),
            "Memory" => results.Select(r => r.PeakMemoryBytes / 1024.0 / 1024.0).ToList(),
            _ => throw new ArgumentException($"Unknown metric: {metricName}")
        };

        var unit = metricName switch
        {
            "Runtime" => "ms",
            "Memory" => "MB",
            _ => ""
        };

        Console.WriteLine($"\n{metricName} Trend (last {results.Count} runs):");
        Console.WriteLine();

        var min = values.Min();
        var max = values.Max();
        var range = max - min;

        if (range == 0)
        {
            Console.WriteLine("All values are identical");
            return;
        }

        // Create chart
        var chart = new char[height, width];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                chart[y, x] = ' ';
            }
        }

        // Plot points
        for (int i = 0; i < values.Count; i++)
        {
            int x = (int)((double)i / (values.Count - 1) * (width - 1));
            double normalized = (values[i] - min) / range;
            int y = height - 1 - (int)(normalized * (height - 1));

            if (x >= 0 && x < width && y >= 0 && y < height)
            {
                chart[y, x] = '●';

                // Draw line to previous point
                if (i > 0)
                {
                    int prevX = (int)((double)(i - 1) / (values.Count - 1) * (width - 1));
                    double prevNormalized = (values[i - 1] - min) / range;
                    int prevY = height - 1 - (int)(prevNormalized * (height - 1));

                    DrawLine(chart, prevX, prevY, x, y);
                }
            }
        }

        // Render chart with y-axis on right
        Console.Write("  ");
        for (int x = 0; x < width; x++)
        {
            char c = chart[0, x];
            if (c == '●')
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write(c);
                Console.ResetColor();
            }
            else if (c == '─')
            {
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.Write(c);
                Console.ResetColor();
            }
            else
            {
                Console.Write(c);
            }
        }
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write(" ┤ ");
        Console.ResetColor();
        Console.WriteLine($"{max:F2}{unit} (highest)");

        for (int y = 1; y < height; y++)
        {
            Console.Write("  ");
            for (int x = 0; x < width; x++)
            {
                char c = chart[y, x];
                if (c == '●')
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write(c);
                    Console.ResetColor();
                }
                else if (c == '─')
                {
                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    Console.Write(c);
                    Console.ResetColor();
                }
                else
                {
                    Console.Write(c);
                }
            }
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(" │");
            Console.ResetColor();
        }

        Console.Write("  ");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write(new string('─', width));
        Console.Write("└┘ ");
        Console.ResetColor();
        Console.WriteLine($"{min:F2}{unit} (lowest)");

        Console.Write("  ");
        Console.Write("First");
        Console.Write(new string(' ', width - 10));
        Console.WriteLine("Last");
        Console.WriteLine();
    }

    public static void RenderSparkline(List<BenchmarkResult> results, string metricName)
    {
        if (results.Count == 0) return;

        var values = metricName switch
        {
            "Runtime" => results.Select(r => r.RuntimeMs).ToList(),
            "Memory" => results.Select(r => r.PeakMemoryBytes / 1024.0 / 1024.0).ToList(),
            _ => throw new ArgumentException($"Unknown metric: {metricName}")
        };

        var unit = metricName switch
        {
            "Runtime" => "ms",
            "Memory" => "MB",
            _ => ""
        };

        var min = values.Min();
        var max = values.Max();
        var range = max - min;

        Console.Write($"  {metricName,-12}: ");

        if (range == 0)
        {
            Console.Write(new string('▄', values.Count));
            Console.WriteLine($"  {min:F1}{unit} (constant)");
            return;
        }

        var sparkChars = new[] { '▁', '▂', '▃', '▄', '▅', '▆', '▇', '█' };

        foreach (var value in values)
        {
            var normalized = (value - min) / range;
            var index = (int)(normalized * (sparkChars.Length - 1));
            index = Math.Clamp(index, 0, sparkChars.Length - 1);

            // Color based on value
            if (normalized > 0.7)
                Console.ForegroundColor = ConsoleColor.Red;
            else if (normalized > 0.4)
                Console.ForegroundColor = ConsoleColor.Yellow;
            else
                Console.ForegroundColor = ConsoleColor.Green;

            Console.Write(sparkChars[index]);
        }

        Console.ResetColor();
        Console.WriteLine($"  {values.First():F1}{unit} → {values.Last():F1}{unit}");
    }

    private static void DrawLine(char[,] chart, int x0, int y0, int x1, int y1)
    {
        int dx = Math.Abs(x1 - x0);
        int dy = Math.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            if (x0 >= 0 && x0 < chart.GetLength(1) && y0 >= 0 && y0 < chart.GetLength(0))
            {
                if (chart[y0, x0] != '●')
                    chart[y0, x0] = '─';
            }

            if (x0 == x1 && y0 == y1) break;

            int e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }
    }

    public static void RenderHistogram(List<double> values, string title, string unit = "", int width = 40)
    {
        if (values.Count == 0) return;

        Console.WriteLine($"\n{title}:");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"Shows distribution of {values.Count} data points");
        Console.ResetColor();
        Console.WriteLine();

        var min = values.Min();
        var max = values.Max();
        var range = max - min;

        if (range == 0)
        {
            Console.WriteLine("All values are identical");
            return;
        }

        int buckets = Math.Min(10, values.Count);
        var histogram = new int[buckets];

        foreach (var value in values)
        {
            var normalized = (value - min) / range;
            var bucket = (int)(normalized * (buckets - 1));
            bucket = Math.Clamp(bucket, 0, buckets - 1);
            histogram[bucket]++;
        }

        var maxCount = histogram.Max();

        for (int i = 0; i < buckets; i++)
        {
            var bucketMin = min + (range * i / buckets);
            var bucketMax = min + (range * (i + 1) / buckets);

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($"{bucketMin,6:F1}-{bucketMax,6:F1}{unit} │");
            Console.ResetColor();

            var barWidth = (int)((double)histogram[i] / maxCount * width);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(new string('█', barWidth));
            Console.ResetColor();
            Console.WriteLine($" {histogram[i]} runs");
        }
        Console.WriteLine();
    }
}
