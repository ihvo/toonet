namespace TooNet.Benchmarks;

[MemoryDiagnoser]
[JsonExporter]
public class DeepNestingBenchmarks
{
    [Params(2, 5, 10)]
    public int NestingDepth { get; set; }

    private object _nestedObject = null!;

    [GlobalSetup]
    public void Setup()
    {
        _nestedObject = DataGenerator.GenerateNestedStructure(NestingDepth);
    }

    [Benchmark(Baseline = true)]
    public string SystemTextJson() =>
        System.Text.Json.JsonSerializer.Serialize(_nestedObject);

    [Benchmark]
    public string TooNet() =>
        TooNetSerializer.Serialize(_nestedObject);

    [Benchmark]
    public byte[] SystemTextJson_Utf8() =>
        System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(_nestedObject);

    [Benchmark]
    public byte[] TooNet_Utf8() =>
        TooNetSerializer.SerializeToUtf8Bytes(_nestedObject);
}
