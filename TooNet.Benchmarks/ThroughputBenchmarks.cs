namespace TooNet.Benchmarks;

[SimpleJob(RuntimeMoniker.Net90)]
[ThreadingDiagnoser]
[MemoryDiagnoser]
[JsonExporter]
public class ThroughputBenchmarks
{
    private List<SimplePrimitive> _smallObjects = null!;
    private List<Product> _mediumProducts = null!;
    private List<Order> _largeOrders = null!;
    private byte[] _largeDataset = null!;
    private ProductCatalog _batchCatalog = null!;

    [Params(100, 1000)]
    public int IterationCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _smallObjects = DataGenerator.GenerateSimplePrimitives(100);

        _mediumProducts = new List<Product>();
        for (int i = 0; i < 50; i++)
        {
            _mediumProducts.Add(new Product
            {
                Sku = $"SKU-{i:D5}",
                Name = $"Product {i}",
                Price = (decimal)(DataGenerator.Random.NextDouble() * 1000),
                Tags = new[] { "tag1", "tag2", "tag3" }
            });
        }

        _largeOrders = new List<Order>();
        for (int i = 0; i < 10; i++)
        {
            _largeOrders.Add(DataGenerator.GenerateComplexOrder(20));
        }

        _batchCatalog = DataGenerator.GenerateProductCatalog(100);
        _largeDataset = Encoding.UTF8.GetBytes(
            System.Text.Json.JsonSerializer.Serialize(DataGenerator.GenerateProductCatalog(1000))
        );
    }

    // High-frequency small object serialization (simulates API responses)
    [Benchmark]
    public void SmallObject_HighFrequency_SystemTextJson()
    {
        for (int i = 0; i < IterationCount; i++)
        {
            _ = System.Text.Json.JsonSerializer.Serialize(_smallObjects[i % _smallObjects.Count]);
        }
    }

    [Benchmark]
    public void SmallObject_HighFrequency_TooNet()
    {
        for (int i = 0; i < IterationCount; i++)
        {
            _ = TooNetSerializer.Serialize(_smallObjects[i % _smallObjects.Count]);
        }
    }

    // Batch processing simulation (simulates data export)
    [Benchmark]
    public string BatchProcess_SystemTextJson()
    {
        var result = "";
        for (int i = 0; i < 10; i++)
        {
            result = System.Text.Json.JsonSerializer.Serialize(_batchCatalog);
        }
        return result;
    }

    [Benchmark]
    public string BatchProcess_TooNet()
    {
        var result = "";
        for (int i = 0; i < 10; i++)
        {
            result = TooNetSerializer.Serialize(_batchCatalog);
        }
        return result;
    }

    // Large single operations (simulates report generation)
    [Benchmark]
    public long LargeOperation_SystemTextJson()
    {
        var largeOrders = new List<Order>();
        for (int i = 0; i < 100; i++)
        {
            largeOrders.Add(DataGenerator.GenerateComplexOrder(25));
        }
        var json = System.Text.Json.JsonSerializer.Serialize(largeOrders);
        return Encoding.UTF8.GetByteCount(json);
    }

    [Benchmark]
    public long LargeOperation_TooNet()
    {
        var largeOrders = new List<Order>();
        for (int i = 0; i < 100; i++)
        {
            largeOrders.Add(DataGenerator.GenerateComplexOrder(25));
        }
        var toonet = TooNetSerializer.Serialize(largeOrders);
        return Encoding.UTF8.GetByteCount(toonet);
    }

    // Measure MB/s throughput with medium-sized objects
    [Benchmark]
    public long MegabytesPerSecond_SystemTextJson()
    {
        long totalBytes = 0;
        for (int i = 0; i < 100; i++)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(_mediumProducts);
            totalBytes += Encoding.UTF8.GetByteCount(json);
        }
        return totalBytes;
    }

    [Benchmark]
    public long MegabytesPerSecond_TooNet()
    {
        long totalBytes = 0;
        for (int i = 0; i < 100; i++)
        {
            var toonet = TooNetSerializer.Serialize(_mediumProducts);
            totalBytes += Encoding.UTF8.GetByteCount(toonet);
        }
        return totalBytes;
    }

    // Concurrent serialization (simulates multi-threaded server)
    [Benchmark]
    public void Concurrent_SystemTextJson()
    {
        Parallel.For(0, 10, i =>
        {
            for (int j = 0; j < 10; j++)
            {
                _ = System.Text.Json.JsonSerializer.Serialize(_mediumProducts);
            }
        });
    }

    [Benchmark]
    public void Concurrent_TooNet()
    {
        Parallel.For(0, 10, i =>
        {
            for (int j = 0; j < 10; j++)
            {
                _ = TooNetSerializer.Serialize(_mediumProducts);
            }
        });
    }

    // Sustained string allocation pressure
    [Benchmark]
    public void SustainedStringAllocation_SystemTextJson()
    {
        for (int i = 0; i < 200; i++)
        {
            var obj = _smallObjects[i % _smallObjects.Count];
            _ = System.Text.Json.JsonSerializer.Serialize(obj);
        }
    }

    [Benchmark]
    public void SustainedStringAllocation_TooNet()
    {
        for (int i = 0; i < 200; i++)
        {
            var obj = _smallObjects[i % _smallObjects.Count];
            _ = TooNetSerializer.Serialize(obj);
        }
    }

    // Mixed workload (simulates real-world server load)
    [Benchmark]
    public void MixedWorkload_SystemTextJson()
    {
        for (int i = 0; i < 50; i++)
        {
            _ = System.Text.Json.JsonSerializer.Serialize(_smallObjects[i % _smallObjects.Count]);

            if (i % 5 == 0)
            {
                _ = System.Text.Json.JsonSerializer.Serialize(_mediumProducts);
            }

            if (i % 10 == 0)
            {
                _ = System.Text.Json.JsonSerializer.Serialize(_largeOrders[0]);
            }
        }
    }

    [Benchmark]
    public void MixedWorkload_TooNet()
    {
        for (int i = 0; i < 50; i++)
        {
            _ = TooNetSerializer.Serialize(_smallObjects[i % _smallObjects.Count]);

            if (i % 5 == 0)
            {
                _ = TooNetSerializer.Serialize(_mediumProducts);
            }

            if (i % 10 == 0)
            {
                _ = TooNetSerializer.Serialize(_largeOrders[0]);
            }
        }
    }
}
