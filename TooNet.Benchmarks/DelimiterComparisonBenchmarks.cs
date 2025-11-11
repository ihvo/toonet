namespace TooNet.Benchmarks;

[MemoryDiagnoser]
public class DelimiterComparisonBenchmarks
{
    [Params(100, 1000)]
    public int DataSize { get; set; }

    private List<SalesRecord> _data = null!;
    private TooNetSerializerOptions _commaOptions = null!;
    private TooNetSerializerOptions _tabOptions = null!;
    private TooNetSerializerOptions _pipeOptions = null!;

    [GlobalSetup]
    public void Setup()
    {
        _data = DataGenerator.GenerateSalesRecords(DataSize);

        _commaOptions = new TooNetSerializerOptions { DefaultDelimiter = Delimiter.Comma };
        _tabOptions = new TooNetSerializerOptions { DefaultDelimiter = Delimiter.Tab };
        _pipeOptions = new TooNetSerializerOptions { DefaultDelimiter = Delimiter.Pipe };
    }

    [Benchmark(Baseline = true)]
    public string Delimiter_Comma() => TooNetSerializer.Serialize(_data, _commaOptions);

    [Benchmark]
    public string Delimiter_Tab() => TooNetSerializer.Serialize(_data, _tabOptions);

    [Benchmark]
    public string Delimiter_Pipe() => TooNetSerializer.Serialize(_data, _pipeOptions);

    [GlobalCleanup]
    public void ReportComparison()
    {
        var commaOutput = TooNetSerializer.Serialize(_data, _commaOptions);
        var tabOutput = TooNetSerializer.Serialize(_data, _tabOptions);
        var pipeOutput = TooNetSerializer.Serialize(_data, _pipeOptions);

        var commaTokens = TokenCounter.EstimateToonTokens(commaOutput);
        var tabTokens = TokenCounter.EstimateToonTokens(tabOutput);
        var pipeTokens = TokenCounter.EstimateToonTokens(pipeOutput);

        Console.WriteLine("\n=== Delimiter Comparison ===");
        Console.WriteLine($"Dataset: {DataSize} SalesRecords");
        Console.WriteLine();
        Console.WriteLine($"Comma (,):");
        Console.WriteLine($"  Size: {commaOutput.Length} chars");
        Console.WriteLine($"  Tokens: {commaTokens}");
        Console.WriteLine();
        Console.WriteLine($"Tab (\\t):");
        Console.WriteLine($"  Size: {tabOutput.Length} chars");
        Console.WriteLine($"  Tokens: {tabTokens}");
        Console.WriteLine($"  vs Comma: {((tabTokens - commaTokens) / (double)commaTokens * 100):+0.0;-0.0}%");
        Console.WriteLine();
        Console.WriteLine($"Pipe (|):");
        Console.WriteLine($"  Size: {pipeOutput.Length} chars");
        Console.WriteLine($"  Tokens: {pipeTokens}");
        Console.WriteLine($"  vs Comma: {((pipeTokens - commaTokens) / (double)commaTokens * 100):+0.0;-0.0}%");
        Console.WriteLine();

        var bestDelimiter = commaTokens <= tabTokens && commaTokens <= pipeTokens ? "Comma"
            : tabTokens <= pipeTokens ? "Tab" : "Pipe";
        Console.WriteLine($"Most efficient delimiter: {bestDelimiter}");
    }
}
