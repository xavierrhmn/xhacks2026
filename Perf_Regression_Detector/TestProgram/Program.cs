using System;
using System.Collections.Generic;
using System.Threading;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("TestProgram starting...");
        
        int version = args.Length > 0 && int.TryParse(args[0], out int v) ? v : 1;
        
        switch (version)
        {
            case 1:
                RunVersion1();
                break;
            case 2:
                RunVersion2_SlowerRuntime();
                break;
            case 3:
                RunVersion3_MoreMemory();
                break;
            case 4:
                RunVersion4_MoreGC();
                break;
            default:
                RunVersion1();
                break;
        }
        
        Console.WriteLine("TestProgram completed");
    }
    
    // Baseline - fast and efficient
    static void RunVersion1()
    {
        var list = new List<int>(1_000_000);
        for (int i = 0; i < 1_000_000; i++)
        {
            list.Add(i * 2);
        }
        Thread.Sleep(100);
        Console.WriteLine($"Version 1: Processed {list.Count} items");
    }
    
    // Slower runtime - 2x the work
    static void RunVersion2_SlowerRuntime()
    {
        var list = new List<int>(2_000_000);
        for (int i = 0; i < 2_000_000; i++)
        {
            list.Add(i * 2);
        }
        Thread.Sleep(200);
        Console.WriteLine($"Version 2: Processed {list.Count} items (slower)");
    }
    
    // More memory allocation
    static void RunVersion3_MoreMemory()
    {
        var lists = new List<List<int>>();
        for (int i = 0; i < 100; i++)
        {
            var list = new List<int>(100_000);
            for (int j = 0; j < 100_000; j++)
            {
                list.Add(j);
            }
            lists.Add(list);
        }
        Thread.Sleep(100);
        Console.WriteLine($"Version 3: Created {lists.Count} lists (more memory)");
    }
    
    // High GC pressure - lots of short-lived objects
    static void RunVersion4_MoreGC()
    {
        for (int i = 0; i < 10_000; i++)
        {
            var temp = new string('x', 1000);
            var temp2 = new byte[10_000];
        }
        Thread.Sleep(100);
        Console.WriteLine("Version 4: High GC pressure");
    }
}