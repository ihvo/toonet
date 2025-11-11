namespace TooNet.Benchmarks;

[MemoryDiagnoser]
[JsonExporter]
public class SimpleObjectBenchmarks
{
    private SimplePrimitive _simplePrimitive = null!;
    private SimpleNested _simpleNested = null!;
    private SimpleWithNulls _simpleWithNulls = null!;
    private SimpleWithDates _simpleWithDates = null!;

    [GlobalSetup]
    public void Setup()
    {
        _simplePrimitive = DataGenerator.GenerateSimplePrimitive();
        _simpleNested = new SimpleNested
        {
            Id = 1,
            Name = "Nested Object",
            Details = _simplePrimitive,
            CreatedAt = DateTime.UtcNow
        };
        _simpleWithNulls = new SimpleWithNulls
        {
            Id = 1,
            Name = "Test",
            Age = null,
            Score = 95.5,
            IsVerified = null
        };
        _simpleWithDates = new SimpleWithDates
        {
            Id = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null,
            Timestamp = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7)
        };
    }

    [Benchmark(Baseline = true)]
    public string SimplePrimitive_SystemTextJson() => System.Text.Json.JsonSerializer.Serialize(_simplePrimitive);

    [Benchmark]
    public string SimplePrimitive_TooNet() => TooNetSerializer.Serialize(_simplePrimitive);

    [Benchmark]
    public byte[] SimplePrimitive_SystemTextJson_Utf8() => System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(_simplePrimitive);

    [Benchmark]
    public byte[] SimplePrimitive_TooNet_Utf8() => Encoding.UTF8.GetBytes(TooNetSerializer.Serialize(_simplePrimitive));

    [Benchmark]
    public string SimpleNested_SystemTextJson() => System.Text.Json.JsonSerializer.Serialize(_simpleNested);

    [Benchmark]
    public string SimpleNested_TooNet() => TooNetSerializer.Serialize(_simpleNested);

    [Benchmark]
    public string SimpleWithNulls_SystemTextJson() => System.Text.Json.JsonSerializer.Serialize(_simpleWithNulls);

    [Benchmark]
    public string SimpleWithNulls_TooNet() => TooNetSerializer.Serialize(_simpleWithNulls);

    [Benchmark]
    public string SimpleWithDates_SystemTextJson() => System.Text.Json.JsonSerializer.Serialize(_simpleWithDates);

    [Benchmark]
    public string SimpleWithDates_TooNet() => TooNetSerializer.Serialize(_simpleWithDates);

    [GlobalCleanup]
    public void Cleanup()
    {
        var primitiveJson = System.Text.Json.JsonSerializer.Serialize(_simplePrimitive);
        var primitiveToon = TooNetSerializer.Serialize(_simplePrimitive);
        var primitiveComparison = TokenCounter.CompareFormats(primitiveJson, primitiveToon);

        var nestedJson = System.Text.Json.JsonSerializer.Serialize(_simpleNested);
        var nestedToon = TooNetSerializer.Serialize(_simpleNested);
        var nestedComparison = TokenCounter.CompareFormats(nestedJson, nestedToon);

        Console.WriteLine("\n=== Token Comparison ===");
        Console.WriteLine($"SimplePrimitive: {primitiveComparison.ReductionPercentage:F1}% reduction ({primitiveComparison.TokensSaved} tokens)");
        Console.WriteLine($"SimpleNested: {nestedComparison.ReductionPercentage:F1}% reduction ({nestedComparison.TokensSaved} tokens)");
    }
}
