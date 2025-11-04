using System.Text;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using Newtonsoft.Json;
using TooNet.Benchmarks.Data;

namespace TooNet.Benchmarks.Benchmarks;

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
    public string NewtonsoftJson() =>
        JsonConvert.SerializeObject(_nestedObject);

    [Benchmark]
    public string TooNet() =>
        TooNetSerializer.Serialize(_nestedObject);

    [Benchmark]
    public byte[] SystemTextJson_Utf8() =>
        System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(_nestedObject);

    [Benchmark]
    public byte[] NewtonsoftJson_Utf8() =>
        Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(_nestedObject));

    [Benchmark]
    public byte[] TooNet_Utf8() =>
        TooNetSerializer.SerializeToUtf8Bytes(_nestedObject);
}
