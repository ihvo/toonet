namespace TooNet.Benchmarks;

[MemoryDiagnoser]
public class ArrayFormatBenchmarks
{
    [Params(10, 50, 100)]
    public int ItemCount { get; set; }

    private List<Product> _products = null!;
    private TooNetSerializerOptions _autoOptions = null!;
    private TooNetSerializerOptions _inlineOptions = null!;
    private TooNetSerializerOptions _tabularOptions = null!;
    private TooNetSerializerOptions _listOptions = null!;

    [GlobalSetup]
    public void Setup()
    {
        var catalog = DataGenerator.GenerateProductCatalog(ItemCount);
        _products = catalog.Products;

        _autoOptions = new TooNetSerializerOptions { ArrayMode = ArrayFormatMode.Auto };
        _inlineOptions = new TooNetSerializerOptions { ArrayMode = ArrayFormatMode.Inline };
        _tabularOptions = new TooNetSerializerOptions { ArrayMode = ArrayFormatMode.Tabular };
        _listOptions = new TooNetSerializerOptions { ArrayMode = ArrayFormatMode.List };
    }

    [Benchmark(Baseline = true)]
    public string Format_Auto() => TooNetSerializer.Serialize(_products, _autoOptions);

    [Benchmark]
    public string Format_Inline() => TooNetSerializer.Serialize(_products, _inlineOptions);

    [Benchmark]
    public string Format_Tabular() => TooNetSerializer.Serialize(_products, _tabularOptions);

    [Benchmark]
    public string Format_List() => TooNetSerializer.Serialize(_products, _listOptions);

    [GlobalCleanup]
    public void ReportComparison()
    {
        var autoOutput = TooNetSerializer.Serialize(_products, _autoOptions);
        var inlineOutput = TooNetSerializer.Serialize(_products, _inlineOptions);
        var tabularOutput = TooNetSerializer.Serialize(_products, _tabularOptions);
        var listOutput = TooNetSerializer.Serialize(_products, _listOptions);

        var autoTokens = TokenCounter.EstimateToonTokens(autoOutput);
        var inlineTokens = TokenCounter.EstimateToonTokens(inlineOutput);
        var tabularTokens = TokenCounter.EstimateToonTokens(tabularOutput);
        var listTokens = TokenCounter.EstimateToonTokens(listOutput);

        Console.WriteLine("\n=== Array Format Comparison ===");
        Console.WriteLine($"Dataset: {ItemCount} Products");
        Console.WriteLine();
        Console.WriteLine($"Auto:");
        Console.WriteLine($"  Size: {autoOutput.Length} chars");
        Console.WriteLine($"  Tokens: {autoTokens}");
        Console.WriteLine();
        Console.WriteLine($"Inline:");
        Console.WriteLine($"  Size: {inlineOutput.Length} chars");
        Console.WriteLine($"  Tokens: {inlineTokens}");
        Console.WriteLine($"  vs Auto: {((inlineTokens - autoTokens) / (double)autoTokens * 100):+0.0;-0.0}%");
        Console.WriteLine();
        Console.WriteLine($"Tabular:");
        Console.WriteLine($"  Size: {tabularOutput.Length} chars");
        Console.WriteLine($"  Tokens: {tabularTokens}");
        Console.WriteLine($"  vs Auto: {((tabularTokens - autoTokens) / (double)autoTokens * 100):+0.0;-0.0}%");
        Console.WriteLine();
        Console.WriteLine($"List:");
        Console.WriteLine($"  Size: {listOutput.Length} chars");
        Console.WriteLine($"  Tokens: {listTokens}");
        Console.WriteLine($"  vs Auto: {((listTokens - autoTokens) / (double)autoTokens * 100):+0.0;-0.0}%");
        Console.WriteLine();

        var minTokens = Math.Min(Math.Min(autoTokens, inlineTokens), Math.Min(tabularTokens, listTokens));
        var bestFormat = minTokens == autoTokens ? "Auto"
            : minTokens == inlineTokens ? "Inline"
            : minTokens == tabularTokens ? "Tabular" : "List";
        Console.WriteLine($"Most efficient format: {bestFormat} ({minTokens} tokens)");
    }
}
