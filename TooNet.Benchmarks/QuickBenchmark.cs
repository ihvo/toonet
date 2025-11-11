namespace TooNet.Benchmarks;

public static class QuickBenchmark
{
    public static void Run()
    {
        Console.WriteLine("\n" + new string('=', 60));
        Console.WriteLine("TooNet Benchmark Results Summary");
        Console.WriteLine(new string('=', 60));

        // Test different data types
        RunTest("Simple Object", DataGenerator.GenerateSimplePrimitive(), 10000);
        RunTest("10 Item Collection", DataGenerator.GenerateSimplePrimitives(10), 5000);
        RunTest("100 Item Collection", DataGenerator.GenerateSimplePrimitives(100), 500);
        RunTest("Complex Order", DataGenerator.GenerateOrder(5), 2000);
        RunTest("100 Sales Records", DataGenerator.GenerateSalesRecords(100), 500);

        Console.WriteLine(new string('=', 60));
    }

    private static void RunTest<T>(string name, T data, int iterations)
    {
        var sw = new Stopwatch();

        // Warmup
        for (int i = 0; i < 100; i++)
        {
            _ = System.Text.Json.JsonSerializer.Serialize(data);
            _ = TooNetSerializer.Serialize(data);
        }

        // Test System.Text.Json
        sw.Restart();
        string jsonResult = "";
        for (int i = 0; i < iterations; i++)
        {
            jsonResult = System.Text.Json.JsonSerializer.Serialize(data);
        }
        sw.Stop();
        var jsonTime = sw.Elapsed.TotalMilliseconds;

        // Test TooNet
        sw.Restart();
        string toonResult = "";
        for (int i = 0; i < iterations; i++)
        {
            toonResult = TooNetSerializer.Serialize(data);
        }
        sw.Stop();
        var toonTime = sw.Elapsed.TotalMilliseconds;

        // Calculate token metrics
        var comparison = TokenCounter.CompareFormats(jsonResult, toonResult);

        Console.WriteLine($"\n{name} ({iterations:N0} iterations):");
        Console.WriteLine(new string('-', 40));
        Console.WriteLine($"System.Text.Json:  {jsonTime,8:F2}ms | {jsonResult.Length,6} bytes");
        Console.WriteLine($"TooNet:            {toonTime,8:F2}ms | {toonResult.Length,6} bytes");
        Console.WriteLine($"Performance ratio: {toonTime/jsonTime:F2}x vs System.Text.Json");
        Console.WriteLine($"Token reduction:   {comparison.ReductionPercentage:F1}% ({comparison.TokensSaved} tokens saved)");
    }
}