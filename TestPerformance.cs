using System;
using System.Diagnostics;
using System.Text.Json;
using Newtonsoft.Json;
using TooNet;
using TooNet.Benchmarks.Data;
using TooNet.Benchmarks.Models;

public class TestPerformance
{
    public static void Main()
    {
        // Generate test data
        var simplePrimitive = DataGenerator.GenerateSimplePrimitive();
        var simpleOrder = DataGenerator.GenerateOrder(5);
        var products = DataGenerator.GenerateSimplePrimitives(50);
        var salesRecords = DataGenerator.GenerateSalesRecords(100);

        Console.WriteLine("=" + new string('=', 59));
        Console.WriteLine("TooNet Performance & Token Reduction Analysis");
        Console.WriteLine("=" + new string('=', 59));

        // Test 1: Simple object
        TestSerialization("Simple Object", simplePrimitive);

        // Test 2: Complex object
        TestSerialization("Complex Order (5 items)", simpleOrder);

        // Test 3: Collection
        TestSerialization("Product Collection (50 items)", products);

        // Test 4: Tabular data
        TestSerialization("Sales Records (100 rows)", salesRecords);

        Console.WriteLine("=" + new string('=', 59));
    }

    private static void TestSerialization<T>(string name, T data)
    {
        const int warmup = 100;
        const int iterations = 1000;
        var sw = new Stopwatch();

        // Warmup
        for (int i = 0; i < warmup; i++)
        {
            _ = JsonSerializer.Serialize(data);
            _ = TooNetSerializer.Serialize(data);
        }

        // Test System.Text.Json
        sw.Restart();
        string jsonResult = "";
        for (int i = 0; i < iterations; i++)
        {
            jsonResult = JsonSerializer.Serialize(data);
        }
        sw.Stop();
        var jsonTime = sw.ElapsedMilliseconds;

        // Test TooNet
        sw.Restart();
        string toonResult = "";
        for (int i = 0; i < iterations; i++)
        {
            toonResult = TooNetSerializer.Serialize(data);
        }
        sw.Stop();
        var toonTime = sw.ElapsedMilliseconds;

        // Calculate metrics
        var jsonBytes = System.Text.Encoding.UTF8.GetBytes(jsonResult).Length;
        var toonBytes = System.Text.Encoding.UTF8.GetBytes(toonResult).Length;
        var byteReduction = ((double)(jsonBytes - toonBytes) / jsonBytes) * 100;
        var performanceRatio = (double)toonTime / jsonTime;

        // Estimate tokens (simple heuristic: ~3.5 chars per token)
        var jsonTokens = jsonResult.Length / 3.5;
        var toonTokens = toonResult.Length / 3.5;
        var tokenReduction = ((jsonTokens - toonTokens) / jsonTokens) * 100;

        Console.WriteLine($"\n{name}:");
        Console.WriteLine("-" + new string('-', 39));
        Console.WriteLine($"JSON: {jsonBytes,6} bytes | {jsonTime,4}ms | ~{jsonTokens,5:F0} tokens");
        Console.WriteLine($"TOON: {toonBytes,6} bytes | {toonTime,4}ms | ~{toonTokens,5:F0} tokens");
        Console.WriteLine($"Size reduction: {byteReduction,6:F1}% | Token reduction: {tokenReduction,5:F1}%");
        Console.WriteLine($"Performance: {performanceRatio:F2}x of System.Text.Json");

        // Show sample output for first test
        if (name.Contains("Simple"))
        {
            Console.WriteLine($"\nSample outputs:");
            Console.WriteLine($"JSON: {jsonResult.Substring(0, Math.Min(jsonResult.Length, 80))}...");
            Console.WriteLine($"TOON: {toonResult.Substring(0, Math.Min(toonResult.Length, 80))}...");
        }
    }
}