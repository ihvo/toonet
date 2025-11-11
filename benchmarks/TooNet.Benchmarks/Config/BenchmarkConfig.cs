using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Exporters.Json;
using BenchmarkDotNet.Validators;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Order;

namespace TooNet.Benchmarks.Config;

public class BenchmarkConfig : ManualConfig
{
    public static readonly BenchmarkConfig Default = new();

    public BenchmarkConfig()
    {
        // Job Configuration - ShortRunJob for quick validation
        AddJob(Job.ShortRun
            .WithRuntime(CoreRuntime.Core90)
            .WithGcServer(true)
            .WithGcConcurrent(true)
            .AsBaseline());

        // Diagnosers
        AddDiagnoser(MemoryDiagnoser.Default);
        AddDiagnoser(ThreadingDiagnoser.Default);

        // Statistical Columns
        AddColumn(StatisticColumn.Min);
        AddColumn(StatisticColumn.Max);
        AddColumn(StatisticColumn.Median);
        AddColumn(StatisticColumn.P95);
        AddColumn(StatisticColumn.Mean);
        AddColumn(StatisticColumn.Error);
        AddColumn(StatisticColumn.StdDev);
        AddColumn(RankColumn.Arabic);
        AddColumn(BaselineRatioColumn.RatioMean);

        // Exporters for CI/CD
        AddExporter(JsonExporter.Brief);
        AddExporter(JsonExporter.Full);
        AddExporter(MarkdownExporter.GitHub);
        AddExporter(CsvExporter.Default);
        AddExporter(HtmlExporter.Default);

        // Validators
        AddValidator(JitOptimizationsValidator.FailOnError);

        // Order by speed
        Orderer = new DefaultOrderer(SummaryOrderPolicy.FastestToSlowest);

        // Loggers
        AddLogger(ConsoleLogger.Default);
    }
}
