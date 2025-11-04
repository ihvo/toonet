using Newtonsoft.Json;
using TooNet.Benchmarks.Data;
using TooNet.Benchmarks.Utils;

namespace TooNet.Benchmarks.Benchmarks;

[MemoryDiagnoser]
[JsonExporter]
public class TabularDataBenchmarks
{
    [Params(10, 100, 1000)]
    public int RowCount { get; set; }

    private List<SalesRecord> _salesRecords = null!;
    private List<LogEntry> _logEntries = null!;
    private List<StockPrice> _stockPrices = null!;

    [GlobalSetup]
    public void Setup()
    {
        _salesRecords = DataGenerator.GenerateSalesRecords(RowCount);
        _logEntries = DataGenerator.GenerateLogEntries(RowCount);
        _stockPrices = DataGenerator.GenerateStockPrices("AAPL", RowCount);
    }

    [Benchmark(Baseline = true)]
    public string SalesRecords_SystemTextJson() => System.Text.Json.JsonSerializer.Serialize(_salesRecords);

    [Benchmark]
    public string SalesRecords_NewtonsoftJson() => JsonConvert.SerializeObject(_salesRecords);

    [Benchmark]
    public string SalesRecords_TooNet() => TooNetSerializer.Serialize(_salesRecords);

    [Benchmark]
    public string LogEntries_SystemTextJson() => System.Text.Json.JsonSerializer.Serialize(_logEntries);

    [Benchmark]
    public string LogEntries_NewtonsoftJson() => JsonConvert.SerializeObject(_logEntries);

    [Benchmark]
    public string LogEntries_TooNet() => TooNetSerializer.Serialize(_logEntries);

    [Benchmark]
    public string StockPrices_SystemTextJson() => System.Text.Json.JsonSerializer.Serialize(_stockPrices);

    [Benchmark]
    public string StockPrices_NewtonsoftJson() => JsonConvert.SerializeObject(_stockPrices);

    [Benchmark]
    public string StockPrices_TooNet() => TooNetSerializer.Serialize(_stockPrices);

    [GlobalCleanup]
    public void ReportTokenSavings()
    {
        var salesJson = System.Text.Json.JsonSerializer.Serialize(_salesRecords);
        var salesToon = TooNetSerializer.Serialize(_salesRecords);
        var salesComparison = TokenCounter.CompareFormats(salesJson, salesToon);

        var logsJson = System.Text.Json.JsonSerializer.Serialize(_logEntries);
        var logsToon = TooNetSerializer.Serialize(_logEntries);
        var logsComparison = TokenCounter.CompareFormats(logsJson, logsToon);

        var stocksJson = System.Text.Json.JsonSerializer.Serialize(_stockPrices);
        var stocksToon = TooNetSerializer.Serialize(_stockPrices);
        var stocksComparison = TokenCounter.CompareFormats(stocksJson, stocksToon);

        Console.WriteLine("\n=== Tabular Data Token Savings ===");
        Console.WriteLine($"Dataset Size: {RowCount} rows");
        Console.WriteLine();
        Console.WriteLine($"SalesRecords:");
        Console.WriteLine($"  JSON: {salesComparison.JsonTokens} tokens ({salesJson.Length} chars)");
        Console.WriteLine($"  TooNet: {salesComparison.ToonTokens} tokens ({salesToon.Length} chars)");
        Console.WriteLine($"  Reduction: {salesComparison.ReductionPercentage:F1}% ({salesComparison.TokensSaved} tokens saved)");
        Console.WriteLine();
        Console.WriteLine($"LogEntries:");
        Console.WriteLine($"  JSON: {logsComparison.JsonTokens} tokens ({logsJson.Length} chars)");
        Console.WriteLine($"  TooNet: {logsComparison.ToonTokens} tokens ({logsToon.Length} chars)");
        Console.WriteLine($"  Reduction: {logsComparison.ReductionPercentage:F1}% ({logsComparison.TokensSaved} tokens saved)");
        Console.WriteLine();
        Console.WriteLine($"StockPrices:");
        Console.WriteLine($"  JSON: {stocksComparison.JsonTokens} tokens ({stocksJson.Length} chars)");
        Console.WriteLine($"  TooNet: {stocksComparison.ToonTokens} tokens ({stocksToon.Length} chars)");
        Console.WriteLine($"  Reduction: {stocksComparison.ReductionPercentage:F1}% ({stocksComparison.TokensSaved} tokens saved)");
        Console.WriteLine();

        var avgReduction = (salesComparison.ReductionPercentage + logsComparison.ReductionPercentage + stocksComparison.ReductionPercentage) / 3;
        Console.WriteLine($"Average Token Reduction: {avgReduction:F1}%");
    }
}
