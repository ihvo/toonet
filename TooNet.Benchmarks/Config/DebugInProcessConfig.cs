using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Validators;
using BenchmarkDotNet.Toolchains.InProcess.Emit;

namespace TooNet.Benchmarks.Config;

/// <summary>
/// Configuration for debugging benchmarks in-process.
/// Use this when you need to debug benchmark code.
/// </summary>
public class DebugInProcessConfig : ManualConfig
{
    public DebugInProcessConfig()
    {
        // Run in-process for debugging
        AddJob(Job.Default
            .WithToolchain(InProcessEmitToolchain.Instance));

        // Basic diagnostics
        AddDiagnoser(MemoryDiagnoser.Default);

        // Console output
        AddLogger(ConsoleLogger.Default);

        // Disable optimization validator for debug builds
        AddValidator(JitOptimizationsValidator.DontFailOnError);
    }
}