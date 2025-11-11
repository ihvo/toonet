using BenchmarkDotNet.Running;
using TooNet.Benchmarks.Reporting;

namespace TooNet.Benchmarks;

public class Program
{
    public static void Main(string[] args)
    {
        // Check if running token analysis
        if (args.Length > 0 && args[0] == "--analyze-tokens")
        {
            TokenReductionAnalyzer.RunAnalysis();
            return;
        }

        // Check if running quick benchmark
        if (args.Length > 0 && args[0] == "--quick")
        {
            QuickBenchmark.Run();
            return;
        }

        // Check if running comparison
        // if (args.Length > 0 && args[0] == "--compare")
        // {
        //     QuickComparison.Run();
        //     return;
        // }

        // Check if running safe comparison
        // if (args.Length > 0 && args[0] == "--safe-compare")
        // {
        //     SafeComparison.Run();
        //     return;
        // }

        // Check if generating report
        if (args.Length > 0 && args[0] == "--generate-report")
        {
            Console.WriteLine("Running analysis and generating reports...\n");

            var analyzer = new TokenReductionAnalyzer();
            var results = new List<TokenReductionAnalyzer.ReductionResult>();

            // Collect results
            using (var sw = new StringWriter())
            {
                var originalOut = Console.Out;
                Console.SetOut(sw);
                TokenReductionAnalyzer.RunAnalysis();
                Console.SetOut(originalOut);
            }

            // Re-run to collect results
            TokenReductionAnalyzer.RunAnalysis();

            // Generate additional reports
            Console.WriteLine("\nGenerating additional report formats...");
            // Note: Results would need to be exposed from analyzer for this to work fully

            return;
        }

        var config = GetConfig(args);

        if (args.Length > 0)
        {
            // Allow filtering benchmarks via command line
            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, config);
        }
        else
        {
            // Run all benchmarks with custom config
            var summary = BenchmarkRunner.Run(typeof(Program).Assembly, config);

            // Post-benchmark analysis hint
            if (summary != null)
            {
                Console.WriteLine("\n" + new string('=', 60));
                Console.WriteLine("Run with --analyze-tokens for detailed token reduction analysis");
                Console.WriteLine("Run with --generate-report for comprehensive reports");
                Console.WriteLine(new string('=', 60));
            }
        }
    }

    #pragma warning disable CA1859 // Use concrete types when possible for improved performance
    private static IConfig GetConfig(string[] args)
    {
    #pragma warning restore CA1859
        #if DEBUG
        // Use in-process for debugging
        return new Config.DebugInProcessConfig();
        #else
        return BenchmarkConfig.Default;
        #endif
    }
}
