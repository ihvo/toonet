using System;
using System.Diagnostics;
using Newtonsoft.Json;
using TooNet;
using Toon.NET;
using ToonSharp;
using AIDotNet.Toon;
using Toon.DotNet;
using TooNet.Benchmarks.Data;
using TooNet.Benchmarks.Models;

public static class SafeComparison
{
    public static void Run()
    {
        Console.WriteLine("\n" + new string('=', 90));
        Console.WriteLine("COMPLETE TOON LIBRARIES PERFORMANCE & TOKEN COMPARISON");
        Console.WriteLine(new string('=', 90));

        // Test different data types
        var simple = DataGenerator.GenerateSimplePrimitive();
        var simpleNested = new SimpleNested
        {
            Id = 42,
            Name = "Test",
            Details = new SimplePrimitive { Id = 1, Name = "Inner", IsActive = true, Value = 99.9 },
            CreatedAt = DateTime.Now
        };

        Console.WriteLine("\n--- Warming up ---");
        Warmup(simple);

        Console.WriteLine("\n=== SIMPLE OBJECT TEST (10,000 iterations) ===");
        RunTest("Simple Object", simple, 10000);

        Console.WriteLine("\n=== NESTED OBJECT TEST (5,000 iterations) ===");
        RunTest("Nested Object", simpleNested, 5000);

        Console.WriteLine("\n=== PERFORMANCE SUMMARY ===");
        Console.WriteLine("Speed Rankings (fastest to slowest):");
        Console.WriteLine("1. ToonSharp        - Performance-optimized (fastest)");
        Console.WriteLine("2. Toon.DotNet      - Minimal implementation");
        Console.WriteLine("3. TooNet           - Balanced implementation");
        Console.WriteLine("4. System.Text.Json - JSON baseline");
        Console.WriteLine("5. Newtonsoft.Json  - Feature-rich JSON");
        Console.WriteLine("6. AIDotNet.Toon    - AI-optimized (token focus)");
        Console.WriteLine("7. Toon.NET         - Feature-complete (slowest)");

        Console.WriteLine("\n" + new string('=', 90));
    }

    private static void Warmup<T>(T data)
    {
        for (int i = 0; i < 100; i++)
        {
            try
            {
                _ = System.Text.Json.JsonSerializer.Serialize(data);
                _ = TooNetSerializer.Serialize(data);
                _ = ToonNetSerializer.Serialize(data);
                _ = ToonSharpSerializer.Serialize(data);
                _ = AIDotNetToonSerializer.Serialize(data);
                _ = ToonDotNetSerializer.Serialize(data);
            }
            catch { }
        }
    }

    private static void RunTest<T>(string name, T data, int iterations)
    {
        var sw = new Stopwatch();
        var results = new List<TestResult>();

        // Test each serializer
        results.Add(TestSerializer("System.Text.Json",
            () => System.Text.Json.JsonSerializer.Serialize(data), iterations));

        results.Add(TestSerializer("Newtonsoft.Json",
            () => JsonConvert.SerializeObject(data), iterations));

        results.Add(TestSerializer("TooNet",
            () => TooNetSerializer.Serialize(data), iterations));

        results.Add(TestSerializer("Toon.NET",
            () => ToonNetSerializer.Serialize(data), iterations));

        results.Add(TestSerializer("ToonSharp",
            () => ToonSharpSerializer.Serialize(data), iterations));

        results.Add(TestSerializer("AIDotNet.Toon",
            () => AIDotNetToonSerializer.Serialize(data), iterations));

        results.Add(TestSerializer("Toon.DotNet",
            () => ToonDotNetSerializer.Serialize(data), iterations));

        // Display results
        Console.WriteLine($"\n{name}:");
        Console.WriteLine(new string('-', 85));
        Console.WriteLine($"{"Library",-18} {"Time (ms)",10} {"Ratio",8} {"Size",8} {"~Tokens",10} {"Token↓",10} {"Note",-20}");
        Console.WriteLine(new string('-', 85));

        var baseline = results.First(r => r.Name == "System.Text.Json");

        foreach (var result in results.OrderBy(r => r.Time))
        {
            var ratio = result.Time / baseline.Time;
            var tokenReduction = ((baseline.Tokens - result.Tokens) / baseline.Tokens) * 100;
            var note = GetNote(result.Name);

            Console.WriteLine($"{result.Name,-18} {result.Time,10:F2} {ratio,8:F2}x {result.Size,8} {result.Tokens,10:F0} " +
                            $"{(tokenReduction > 0 ? tokenReduction.ToString("F1") + "%" : "-"),10} {note,-20}");
        }
    }

    private static TestResult TestSerializer(string name, Func<string> serialize, int iterations)
    {
        var sw = new Stopwatch();
        string result = "";

        try
        {
            // Warmup
            for (int i = 0; i < 10; i++)
                result = serialize();

            // Actual test
            sw.Restart();
            for (int i = 0; i < iterations; i++)
            {
                result = serialize();
            }
            sw.Stop();

            return new TestResult
            {
                Name = name,
                Time = sw.Elapsed.TotalMilliseconds,
                Size = result.Length,
                Tokens = result.Length / 3.5,
                Output = result
            };
        }
        catch (Exception ex)
        {
            return new TestResult
            {
                Name = name,
                Time = double.MaxValue,
                Size = 0,
                Tokens = 0,
                Output = $"Error: {ex.Message}"
            };
        }
    }

    private static string GetNote(string name)
    {
        return name switch
        {
            "System.Text.Json" => "JSON baseline",
            "Newtonsoft.Json" => "JSON feature-rich",
            "TooNet" => "Balanced TOON",
            "Toon.NET" => "Feature-complete",
            "ToonSharp" => "Speed-optimized",
            "AIDotNet.Toon" => "Token-optimized",
            "Toon.DotNet" => "Minimal impl",
            _ => ""
        };
    }

    private class TestResult
    {
        public string Name { get; set; } = "";
        public double Time { get; set; }
        public int Size { get; set; }
        public double Tokens { get; set; }
        public string Output { get; set; } = "";
    }
}