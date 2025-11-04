using Newtonsoft.Json;

namespace TooNet.Benchmarks.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net90)]
public class PrimitiveBenchmarks
{
    private int _intValue = 42;
    private double _doubleValue = 3.14159;
    private string _stringValue = "Hello, World!";
    private bool _boolValue = true;
    private DateTime _dateValue;

    [GlobalSetup]
    public void Setup()
    {
        _dateValue = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);
    }

    [Benchmark(Baseline = true)]
    public string Int_SystemTextJson() => System.Text.Json.JsonSerializer.Serialize(_intValue);

    [Benchmark]
    public string Int_NewtonsoftJson() => JsonConvert.SerializeObject(_intValue);

    [Benchmark]
    public string Int_TooNet() => TooNetSerializer.Serialize(_intValue);

    [Benchmark]
    public string Double_SystemTextJson() => System.Text.Json.JsonSerializer.Serialize(_doubleValue);

    [Benchmark]
    public string Double_NewtonsoftJson() => JsonConvert.SerializeObject(_doubleValue);

    [Benchmark]
    public string Double_TooNet() => TooNetSerializer.Serialize(_doubleValue);

    [Benchmark]
    public string String_SystemTextJson() => System.Text.Json.JsonSerializer.Serialize(_stringValue);

    [Benchmark]
    public string String_NewtonsoftJson() => JsonConvert.SerializeObject(_stringValue);

    [Benchmark]
    public string String_TooNet() => TooNetSerializer.Serialize(_stringValue);

    [Benchmark]
    public string Bool_SystemTextJson() => System.Text.Json.JsonSerializer.Serialize(_boolValue);

    [Benchmark]
    public string Bool_NewtonsoftJson() => JsonConvert.SerializeObject(_boolValue);

    [Benchmark]
    public string Bool_TooNet() => TooNetSerializer.Serialize(_boolValue);

    [Benchmark]
    public string DateTime_SystemTextJson() => System.Text.Json.JsonSerializer.Serialize(_dateValue);

    [Benchmark]
    public string DateTime_NewtonsoftJson() => JsonConvert.SerializeObject(_dateValue);

    [Benchmark]
    public string DateTime_TooNet() => TooNetSerializer.Serialize(_dateValue);
}
