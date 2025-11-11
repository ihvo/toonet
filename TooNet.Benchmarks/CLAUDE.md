# TooNet Benchmarks

Performance benchmarks for the TooNet serialization library.

## Overview

This benchmark suite compares TooNet's performance against other popular .NET serialization libraries:
- System.Text.Json
- Newtonsoft.Json

## Running Benchmarks

Run all benchmarks:
```bash
dotnet run -c Release
```

Run specific benchmarks by filter:
```bash
dotnet run -c Release --filter *SerializationBenchmark*
```

## Interpreting Results

BenchmarkDotNet will output results showing:
- **Mean**: Average execution time
- **Error**: Half of 99.9% confidence interval
- **StdDev**: Standard deviation of all measurements
- **Allocated**: Memory allocated per operation

Lower values are better for all metrics.

## Output

Results are exported to `BenchmarkDotNet.Artifacts/results/` in multiple formats:
- JSON for programmatic analysis
- HTML for web viewing
- Markdown for documentation

## Adding Benchmarks

Create a new class in the project and mark methods with `[Benchmark]`:
```csharp
public class MyBenchmark
{
    [Benchmark]
    public void TestMethod()
    {
        // Your code here
    }
}
```

See [BenchmarkDotNet documentation](https://benchmarkdotnet.org/) for more details.
