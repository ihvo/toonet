namespace TooNet.Benchmarks.Utils;

public class BenchmarkMetrics
{
    public string TestName { get; set; } = string.Empty;
    public int DataSize { get; set; }

    public double SerializationTimeMs { get; set; }
    public long MemoryAllocatedBytes { get; set; }

    public int JsonTokenCount { get; set; }
    public int ToonTokenCount { get; set; }
    public double TokenReductionPercentage { get; set; }

    public static string FormatComparison(BenchmarkMetrics jsonMetric, BenchmarkMetrics toonMetric)
    {
        var speedup = jsonMetric.SerializationTimeMs / toonMetric.SerializationTimeMs;
        var memoryReduction = ((jsonMetric.MemoryAllocatedBytes - toonMetric.MemoryAllocatedBytes)
            / (double)jsonMetric.MemoryAllocatedBytes) * 100;

        return $"""
            {jsonMetric.TestName} Comparison (n={jsonMetric.DataSize}):
              Time:   JSON {jsonMetric.SerializationTimeMs:F2}ms vs TOON {toonMetric.SerializationTimeMs:F2}ms ({speedup:F2}x)
              Memory: JSON {FormatBytes(jsonMetric.MemoryAllocatedBytes)} vs TOON {FormatBytes(toonMetric.MemoryAllocatedBytes)} ({memoryReduction:F1}% reduction)
              Tokens: JSON {jsonMetric.JsonTokenCount} vs TOON {toonMetric.ToonTokenCount} ({toonMetric.TokenReductionPercentage:F1}% reduction)
            """;
    }

    private static string FormatBytes(long bytes)
    {
        return bytes switch
        {
            < 1024 => $"{bytes} B",
            < 1024 * 1024 => $"{bytes / 1024.0:F1} KB",
            _ => $"{bytes / (1024.0 * 1024):F1} MB"
        };
    }
}
