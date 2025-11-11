namespace TooNet.Benchmarks;

[MemoryDiagnoser]
[JsonExporter]
public class CollectionBenchmarks
{
    private List<SimplePrimitive> _small = null!;
    private List<SimplePrimitive> _medium = null!;
    private List<SimplePrimitive> _large = null!;
    private ProductCatalog _catalog = null!;

    [Params(10, 100, 1000)]
    public int CollectionSize { get; set; }

    private List<SimplePrimitive> _currentCollection = null!;

    [GlobalSetup]
    public void Setup()
    {
        _small = DataGenerator.GenerateSimplePrimitives(10);
        _medium = DataGenerator.GenerateSimplePrimitives(100);
        _large = DataGenerator.GenerateSimplePrimitives(1000);
        _catalog = DataGenerator.GenerateProductCatalog(100);

        _currentCollection = CollectionSize switch
        {
            10 => _small,
            100 => _medium,
            1000 => _large,
            _ => _small
        };
    }

    [Benchmark(Baseline = true)]
    public string Collection_SystemTextJson() => System.Text.Json.JsonSerializer.Serialize(_currentCollection);

    [Benchmark]
    public string Collection_TooNet() => TooNetSerializer.Serialize(_currentCollection);

    [Benchmark]
    public byte[] Collection_SystemTextJson_Utf8() => System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(_currentCollection);

    [Benchmark]
    public byte[] Collection_TooNet_Utf8() => Encoding.UTF8.GetBytes(TooNetSerializer.Serialize(_currentCollection));

    [Benchmark]
    public string ProductCatalog_SystemTextJson() => System.Text.Json.JsonSerializer.Serialize(_catalog);

    [Benchmark]
    public string ProductCatalog_TooNet() => TooNetSerializer.Serialize(_catalog);

    [GlobalCleanup]
    public void Cleanup()
    {
        var collectionJson = System.Text.Json.JsonSerializer.Serialize(_currentCollection);
        var collectionToon = TooNetSerializer.Serialize(_currentCollection);
        var collectionComparison = TokenCounter.CompareFormats(collectionJson, collectionToon);

        var catalogJson = System.Text.Json.JsonSerializer.Serialize(_catalog);
        var catalogToon = TooNetSerializer.Serialize(_catalog);
        var catalogComparison = TokenCounter.CompareFormats(catalogJson, catalogToon);

        Console.WriteLine("\n=== Token Comparison ===");
        Console.WriteLine($"Collection (n={CollectionSize}): {collectionComparison.ReductionPercentage:F1}% reduction ({collectionComparison.TokensSaved} tokens)");
        Console.WriteLine($"ProductCatalog: {catalogComparison.ReductionPercentage:F1}% reduction ({catalogComparison.TokensSaved} tokens)");
    }
}
