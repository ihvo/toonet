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
using TooNet.Benchmarks.Utils;

public static class QuickComparison
{
    public static void Run()
    {
        Console.WriteLine("\n" + new string('=', 80));
        Console.WriteLine("TOON Libraries Performance Comparison");
        Console.WriteLine(new string('=', 80));

        // Test data
        var simple = DataGenerator.GenerateSimplePrimitive();
        var order = DataGenerator.GenerateOrder(5);

        Console.WriteLine("\nWarming up...");
        Warmup(simple);

        Console.WriteLine("\n--- Simple Object Performance (10,000 iterations) ---");
        RunComparison("Simple Object", simple, 10000);

        Console.WriteLine("\n--- Complex Order Performance (2,000 iterations) ---");
        RunComparison("Complex Order", order, 2000);

        Console.WriteLine("\n" + new string('=', 80));
    }

    private static void Warmup<T>(T data)
    {
        for (int i = 0; i < 100; i++)
        {
            _ = System.Text.Json.JsonSerializer.Serialize(data);
            _ = TooNetSerializer.Serialize(data);
            _ = ToonNetSerializer.Serialize(data);
            _ = ToonSharpSerializer.Serialize(data);
            _ = AIDotNetToonSerializer.Serialize(data);
            _ = ToonDotNetSerializer.Serialize(data);
        }
    }

    private static void RunComparison<T>(string name, T data, int iterations)
    {
        var sw = new Stopwatch();
        var results = new List<(string Name, double Time, int Size, double Tokens)>();

        // Test System.Text.Json (baseline)
        sw.Restart();
        string jsonResult = "";
        for (int i = 0; i < iterations; i++)
        {
            jsonResult = System.Text.Json.JsonSerializer.Serialize(data);
        }
        sw.Stop();
        var jsonTime = sw.Elapsed.TotalMilliseconds;
        results.Add(("System.Text.Json", jsonTime, jsonResult.Length, jsonResult.Length / 3.5));

        // Test Newtonsoft.Json
        sw.Restart();
        string newtonsoftResult = "";
        for (int i = 0; i < iterations; i++)
        {
            newtonsoftResult = JsonConvert.SerializeObject(data);
        }
        sw.Stop();
        results.Add(("Newtonsoft.Json", sw.Elapsed.TotalMilliseconds, newtonsoftResult.Length, newtonsoftResult.Length / 3.5));

        // Test TooNet
        sw.Restart();
        string toonetResult = "";
        for (int i = 0; i < iterations; i++)
        {
            toonetResult = TooNetSerializer.Serialize(data);
        }
        sw.Stop();
        results.Add(("TooNet", sw.Elapsed.TotalMilliseconds, toonetResult.Length, toonetResult.Length / 3.5));

        // Test Toon.NET
        sw.Restart();
        string toonNetResult = "";
        for (int i = 0; i < iterations; i++)
        {
            toonNetResult = ToonNetSerializer.Serialize(data);
        }
        sw.Stop();
        results.Add(("Toon.NET", sw.Elapsed.TotalMilliseconds, toonNetResult.Length, toonNetResult.Length / 3.5));

        // Test ToonSharp
        sw.Restart();
        string toonSharpResult = "";
        for (int i = 0; i < iterations; i++)
        {
            toonSharpResult = ToonSharpSerializer.Serialize(data);
        }
        sw.Stop();
        results.Add(("ToonSharp", sw.Elapsed.TotalMilliseconds, toonSharpResult.Length, toonSharpResult.Length / 3.5));

        // Test AIDotNet.Toon
        sw.Restart();
        string aidotnetResult = "";
        for (int i = 0; i < iterations; i++)
        {
            aidotnetResult = AIDotNetToonSerializer.Serialize(data);
        }
        sw.Stop();
        results.Add(("AIDotNet.Toon", sw.Elapsed.TotalMilliseconds, aidotnetResult.Length, aidotnetResult.Length / 3.5));

        // Test Toon.DotNet
        sw.Restart();
        string toonDotNetResult = "";
        for (int i = 0; i < iterations; i++)
        {
            toonDotNetResult = ToonDotNetSerializer.Serialize(data);
        }
        sw.Stop();
        results.Add(("Toon.DotNet", sw.Elapsed.TotalMilliseconds, toonDotNetResult.Length, toonDotNetResult.Length / 3.5));

        // Display results
        Console.WriteLine($"\n{name}:");
        Console.WriteLine(new string('-', 70));
        Console.WriteLine($"{"Library",-18} {"Time (ms)",10} {"Ratio",8} {"Size",8} {"~Tokens",10} {"Token↓",8}");
        Console.WriteLine(new string('-', 70));

        var baselineTime = results[0].Time;
        var baselineTokens = results[0].Tokens;

        foreach (var (libName, time, size, tokens) in results)
        {
            var ratio = time / baselineTime;
            var tokenReduction = ((baselineTokens - tokens) / baselineTokens) * 100;

            Console.WriteLine($"{libName,-18} {time,10:F2} {ratio,8:F2}x {size,8} {tokens,10:F0} {(tokenReduction > 0 ? tokenReduction.ToString("F1") + "%" : ""),8}");
        }
    }
}