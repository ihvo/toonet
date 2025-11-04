using Newtonsoft.Json;
using TooNet.Benchmarks.Data;
using TooNet.Benchmarks.Utils;

namespace TooNet.Benchmarks.Benchmarks;

[MemoryDiagnoser]
public class TokenReductionBenchmarks
{
    private SimplePrimitive _simple = null!;
    private Order _complex = null!;
    private List<SalesRecord> _tabular = null!;

    [GlobalSetup]
    public void Setup()
    {
        _simple = DataGenerator.GenerateSimplePrimitive();
        _complex = DataGenerator.GenerateOrder(5);
        _tabular = DataGenerator.GenerateSalesRecords(100);
    }

    [Benchmark]
    public TokenComparison Simple_TokenReduction()
    {
        var json = System.Text.Json.JsonSerializer.Serialize(_simple);
        var toon = TooNetSerializer.Serialize(_simple);
        return TokenCounter.CompareFormats(json, toon);
    }

    [Benchmark]
    public TokenComparison Complex_TokenReduction()
    {
        var json = System.Text.Json.JsonSerializer.Serialize(_complex);
        var toon = TooNetSerializer.Serialize(_complex);
        return TokenCounter.CompareFormats(json, toon);
    }

    [Benchmark]
    public TokenComparison Tabular_TokenReduction()
    {
        var json = System.Text.Json.JsonSerializer.Serialize(_tabular);
        var toon = TooNetSerializer.Serialize(_tabular);
        return TokenCounter.CompareFormats(json, toon);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        var simpleJson = System.Text.Json.JsonSerializer.Serialize(_simple);
        var simpleToon = TooNetSerializer.Serialize(_simple);
        var simpleComparison = TokenCounter.CompareFormats(simpleJson, simpleToon);

        var complexJson = System.Text.Json.JsonSerializer.Serialize(_complex);
        var complexToon = TooNetSerializer.Serialize(_complex);
        var complexComparison = TokenCounter.CompareFormats(complexJson, complexToon);

        var tabularJson = System.Text.Json.JsonSerializer.Serialize(_tabular);
        var tabularToon = TooNetSerializer.Serialize(_tabular);
        var tabularComparison = TokenCounter.CompareFormats(tabularJson, tabularToon);

        Console.WriteLine("\n=== Token Savings Report ===");
        Console.WriteLine($"Simple Object:");
        Console.WriteLine($"  JSON: {simpleComparison.JsonTokens} tokens");
        Console.WriteLine($"  TooNet: {simpleComparison.ToonTokens} tokens");
        Console.WriteLine($"  Reduction: {simpleComparison.ReductionPercentage:F1}% ({simpleComparison.TokensSaved} tokens saved)");
        Console.WriteLine();
        Console.WriteLine($"Complex Object (Order):");
        Console.WriteLine($"  JSON: {complexComparison.JsonTokens} tokens");
        Console.WriteLine($"  TooNet: {complexComparison.ToonTokens} tokens");
        Console.WriteLine($"  Reduction: {complexComparison.ReductionPercentage:F1}% ({complexComparison.TokensSaved} tokens saved)");
        Console.WriteLine();
        Console.WriteLine($"Tabular Data (100 records):");
        Console.WriteLine($"  JSON: {tabularComparison.JsonTokens} tokens");
        Console.WriteLine($"  TooNet: {tabularComparison.ToonTokens} tokens");
        Console.WriteLine($"  Reduction: {tabularComparison.ReductionPercentage:F1}% ({tabularComparison.TokensSaved} tokens saved)");
    }
}
