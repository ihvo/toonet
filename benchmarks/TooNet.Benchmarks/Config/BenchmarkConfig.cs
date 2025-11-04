using BenchmarkDotNet.Exporters.Json;

namespace TooNet.Benchmarks.Config;

public class BenchmarkConfig : ManualConfig
{
    public static readonly BenchmarkConfig Default = new();

    public BenchmarkConfig()
    {
        // Use a medium-fast job for development iterations
        AddJob(Job.Default
            .WithWarmupCount(1)
            .WithIterationCount(3)
            .WithInvocationCount(1));

        // Export results in multiple formats
        AddExporter(JsonExporter.Full);
        AddExporter(HtmlExporter.Default);
        AddExporter(MarkdownExporter.GitHub);

        // Display relevant statistics
        AddColumn(StatisticColumn.Mean);
        AddColumn(StatisticColumn.Error);
        AddColumn(StatisticColumn.StdDev);
        AddColumn(BaselineRatioColumn.RatioMean);

        // Track memory allocations
        AddDiagnoser(MemoryDiagnoser.Default);

        // Show options
        WithOption(ConfigOptions.DisableOptimizationsValidator, true);
    }
}
