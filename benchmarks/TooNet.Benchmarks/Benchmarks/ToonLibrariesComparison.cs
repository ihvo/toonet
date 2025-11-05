using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Newtonsoft.Json;
using TooNet;
using TooNet.Benchmarks.Data;
using TooNet.Benchmarks.Models;
using Toon.NET;
using ToonSharp;
using AIDotNet.Toon;
using Toon.DotNet;

namespace TooNet.Benchmarks.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net90)]
[JsonExporter]
public class ToonLibrariesComparison
{
    private SimplePrimitive _simplePrimitive = null!;
    private List<SimplePrimitive> _collection = null!;
    private Order _complexOrder = null!;
    private List<SalesRecord> _tabularData = null!;

    [GlobalSetup]
    public void Setup()
    {
        _simplePrimitive = DataGenerator.GenerateSimplePrimitive();
        _collection = DataGenerator.GenerateSimplePrimitives(50);
        _complexOrder = DataGenerator.GenerateOrder(10);
        _tabularData = DataGenerator.GenerateSalesRecords(100);
    }

    #region Simple Object Benchmarks

    [Benchmark(Baseline = true)]
    public string Simple_SystemTextJson() => System.Text.Json.JsonSerializer.Serialize(_simplePrimitive);

    [Benchmark]
    public string Simple_NewtonsoftJson() => JsonConvert.SerializeObject(_simplePrimitive);

    [Benchmark]
    public string Simple_TooNet() => TooNetSerializer.Serialize(_simplePrimitive);

    [Benchmark]
    public string Simple_ToonNET() => ToonNetSerializer.Serialize(_simplePrimitive);

    [Benchmark]
    public string Simple_ToonSharp() => ToonSharpSerializer.Serialize(_simplePrimitive);

    [Benchmark]
    public string Simple_AIDotNetToon() => AIDotNetToonSerializer.Serialize(_simplePrimitive);

    [Benchmark]
    public string Simple_ToonDotNet() => ToonDotNetSerializer.Serialize(_simplePrimitive);

    #endregion

    #region Collection Benchmarks

    [Benchmark]
    public string Collection_SystemTextJson() => System.Text.Json.JsonSerializer.Serialize(_collection);

    [Benchmark]
    public string Collection_NewtonsoftJson() => JsonConvert.SerializeObject(_collection);

    [Benchmark]
    public string Collection_TooNet() => TooNetSerializer.Serialize(_collection);

    [Benchmark]
    public string Collection_ToonNET() => ToonNetSerializer.Serialize(_collection);

    [Benchmark]
    public string Collection_ToonSharp() => ToonSharpSerializer.Serialize(_collection);

    [Benchmark]
    public string Collection_AIDotNetToon() => AIDotNetToonSerializer.Serialize(_collection);

    [Benchmark]
    public string Collection_ToonDotNet() => ToonDotNetSerializer.Serialize(_collection);

    #endregion

    #region Complex Object Benchmarks

    [Benchmark]
    public string Complex_SystemTextJson() => System.Text.Json.JsonSerializer.Serialize(_complexOrder);

    [Benchmark]
    public string Complex_NewtonsoftJson() => JsonConvert.SerializeObject(_complexOrder);

    [Benchmark]
    public string Complex_TooNet() => TooNetSerializer.Serialize(_complexOrder);

    [Benchmark]
    public string Complex_ToonNET() => ToonNetSerializer.Serialize(_complexOrder);

    [Benchmark]
    public string Complex_ToonSharp() => ToonSharpSerializer.Serialize(_complexOrder);

    [Benchmark]
    public string Complex_AIDotNetToon() => AIDotNetToonSerializer.Serialize(_complexOrder);

    [Benchmark]
    public string Complex_ToonDotNet() => ToonDotNetSerializer.Serialize(_complexOrder);

    #endregion

    #region Tabular Data Benchmarks

    [Benchmark]
    public string Tabular_SystemTextJson() => System.Text.Json.JsonSerializer.Serialize(_tabularData);

    [Benchmark]
    public string Tabular_NewtonsoftJson() => JsonConvert.SerializeObject(_tabularData);

    [Benchmark]
    public string Tabular_TooNet() => TooNetSerializer.Serialize(_tabularData);

    [Benchmark]
    public string Tabular_ToonNET() => ToonNetSerializer.Serialize(_tabularData);

    [Benchmark]
    public string Tabular_ToonSharp() => ToonSharpSerializer.Serialize(_tabularData);

    [Benchmark]
    public string Tabular_AIDotNetToon() => AIDotNetToonSerializer.Serialize(_tabularData);

    [Benchmark]
    public string Tabular_ToonDotNet() => ToonDotNetSerializer.Serialize(_tabularData);

    #endregion

    [GlobalCleanup]
    public void Cleanup()
    {
        // Report comparative analysis
        Console.WriteLine("\n" + new string('=', 80));
        Console.WriteLine("TOON Libraries Comparison Summary");
        Console.WriteLine(new string('=', 80));

        var jsonSimple = System.Text.Json.JsonSerializer.Serialize(_simplePrimitive);
        var toonetSimple = TooNetSerializer.Serialize(_simplePrimitive);
        var toonNetSimple = ToonNetSerializer.Serialize(_simplePrimitive);
        var toonSharpSimple = ToonSharpSerializer.Serialize(_simplePrimitive);
        var aidotnetSimple = AIDotNetToonSerializer.Serialize(_simplePrimitive);
        var toonDotNetSimple = ToonDotNetSerializer.Serialize(_simplePrimitive);

        Console.WriteLine("\nOutput Sizes (Simple Object):");
        Console.WriteLine($"JSON:            {jsonSimple.Length,5} bytes");
        Console.WriteLine($"TooNet:          {toonetSimple.Length,5} bytes");
        Console.WriteLine($"Toon.NET:        {toonNetSimple.Length,5} bytes");
        Console.WriteLine($"ToonSharp:       {toonSharpSimple.Length,5} bytes");
        Console.WriteLine($"AIDotNet.Toon:   {aidotnetSimple.Length,5} bytes");
        Console.WriteLine($"Toon.DotNet:     {toonDotNetSimple.Length,5} bytes");

        Console.WriteLine(new string('=', 80));
    }
}