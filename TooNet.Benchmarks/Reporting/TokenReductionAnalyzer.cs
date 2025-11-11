namespace TooNet.Benchmarks.Reporting;

public class TokenReductionAnalyzer
{
    public class ReductionResult
    {
        public string DatasetName { get; set; } = string.Empty;
        public int JsonBytes { get; set; }
        public int JsonTokens { get; set; }
        public int ToonBytes { get; set; }
        public int ToonTokens { get; set; }
        public double ByteReduction { get; set; }
        public double TokenReduction { get; set; }
    }

    public static void RunAnalysis()
    {
        var results = new List<ReductionResult>();

        Console.WriteLine("================================================");
        Console.WriteLine("TOON Format Token Reduction Analysis");
        Console.WriteLine("================================================\n");

        results.Add(AnalyzeSimpleObjects());
        results.Add(AnalyzeCollections());
        results.Add(AnalyzeTabularData());
        results.Add(AnalyzeComplexStructures());

        PrintSummaryReport(results);
        ExportToCsv(results, "benchmark-results.csv");
    }

    private static ReductionResult AnalyzeSimpleObjects()
    {
        Console.WriteLine("Simple Objects (single item):");
        Console.WriteLine("---------------------------");

        var data = TestDataSets.SmallObject;
        var json = JsonSerializer.Serialize(data);
        var toon = TooNetSerializer.Serialize(data);

        return PrintAndReturn("Simple Objects", json, toon);
    }

    private static ReductionResult AnalyzeCollections()
    {
        Console.WriteLine("\nCollections (primitive arrays):");
        Console.WriteLine("---------------------------");

        var data = new { Numbers = new[] { 1, 2, 3, 4, 5 }, Names = new[] { "Alice", "Bob", "Charlie" } };
        var json = JsonSerializer.Serialize(data);
        var toon = TooNetSerializer.Serialize(data);

        return PrintAndReturn("Collections", json, toon);
    }

    private static ReductionResult AnalyzeTabularData()
    {
        Console.WriteLine("\nTabular Data (simple record):");
        Console.WriteLine("---------------------------");

        // Create simple inline data to avoid DateTime serialization issues
        var data = new { Date = "2024-01-01", Product = "Widget", Quantity = 100, Revenue = 1250.50m };
        var json = JsonSerializer.Serialize(data);
        var toon = TooNetSerializer.Serialize(data);

        return PrintAndReturn("Tabular Data", json, toon);
    }

    private static ReductionResult AnalyzeComplexStructures()
    {
        Console.WriteLine("\nComplex Structures (nested object):");
        Console.WriteLine("---------------------------");

        // Create simple nested structure
        var data = new
        {
            Id = 1,
            Name = "Test Item",
            Details = new { Category = "Electronics", Price = 99.99, InStock = true }
        };
        var json = JsonSerializer.Serialize(data);
        var toon = TooNetSerializer.Serialize(data);

        return PrintAndReturn("Complex Structures", json, toon);
    }

    private static ReductionResult PrintAndReturn(string name, string json, string toon)
    {
        var jsonBytes = Encoding.UTF8.GetByteCount(json);
        var jsonTokens = TokenCounter.EstimateJsonTokens(json);
        var toonBytes = Encoding.UTF8.GetByteCount(toon);
        var toonTokens = TokenCounter.EstimateToonTokens(toon);

        var byteReduction = jsonBytes > 0 ? ((jsonBytes - toonBytes) / (double)jsonBytes) * 100 : 0;
        var tokenReduction = jsonTokens > 0 ? ((jsonTokens - toonTokens) / (double)jsonTokens) * 100 : 0;

        Console.WriteLine($"JSON:  {jsonBytes,8:N0} bytes ({jsonTokens,5:N0} tokens)");
        Console.WriteLine($"TOON:  {toonBytes,8:N0} bytes ({toonTokens,5:N0} tokens)");
        Console.WriteLine($"Reduction: {byteReduction:F1}% bytes, {tokenReduction:F1}% tokens");

        return new ReductionResult
        {
            DatasetName = name,
            JsonBytes = jsonBytes,
            JsonTokens = jsonTokens,
            ToonBytes = toonBytes,
            ToonTokens = toonTokens,
            ByteReduction = byteReduction,
            TokenReduction = tokenReduction
        };
    }

    private static void PrintSummaryReport(List<ReductionResult> results)
    {
        Console.WriteLine("\n================================================");
        Console.WriteLine("Summary Statistics");
        Console.WriteLine("================================================");

        var avgByteReduction = results.Average(r => r.ByteReduction);
        var avgTokenReduction = results.Average(r => r.TokenReduction);
        var totalJsonBytes = results.Sum(r => r.JsonBytes);
        var totalToonBytes = results.Sum(r => r.ToonBytes);
        var totalBytesSaved = totalJsonBytes - totalToonBytes;

        Console.WriteLine($"Average Byte Reduction:  {avgByteReduction:F1}%");
        Console.WriteLine($"Average Token Reduction: {avgTokenReduction:F1}%");
        Console.WriteLine($"Total Bytes Saved:       {totalBytesSaved:N0}");
        Console.WriteLine("================================================\n");
    }

    private static void ExportToCsv(List<ReductionResult> results, string filename)
    {
        var csv = new StringBuilder();
        csv.AppendLine("Dataset,JSON Bytes,JSON Tokens,TOON Bytes,TOON Tokens,Byte Reduction %,Token Reduction %");

        foreach (var result in results)
        {
            csv.AppendLine($"{result.DatasetName},{result.JsonBytes},{result.JsonTokens}," +
                          $"{result.ToonBytes},{result.ToonTokens}," +
                          $"{result.ByteReduction:F2},{result.TokenReduction:F2}");
        }

        File.WriteAllText(filename, csv.ToString());
        Console.WriteLine($"Results exported to {filename}");
    }
}
