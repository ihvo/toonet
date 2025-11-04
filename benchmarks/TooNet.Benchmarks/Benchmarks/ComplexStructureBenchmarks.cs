using System.Text;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using Newtonsoft.Json;
using TooNet.Benchmarks.Data;
using TooNet.Benchmarks.Models;

namespace TooNet.Benchmarks.Benchmarks;

[MemoryDiagnoser]
[JsonExporter]
public class ComplexStructureBenchmarks
{
    private Order _simpleOrder = null!;
    private Order _complexOrder = null!;
    private Customer _customerWithOrders = null!;

    [GlobalSetup]
    public void Setup()
    {
        _simpleOrder = DataGenerator.GenerateOrder(2);
        _complexOrder = DataGenerator.GenerateComplexOrder(10);

        _customerWithOrders = new Customer
        {
            Id = "1",
            Name = "John Doe",
            Email = "john@example.com",
            Type = CustomerType.Premium,
            Addresses = new List<Address>
            {
                StringGenerator.GenerateAddress(),
                StringGenerator.GenerateAddress()
            },
            Orders = new List<Order> { _simpleOrder, _complexOrder }
        };
    }

    [Benchmark(Baseline = true)]
    public string SimpleOrder_SystemTextJson() =>
        System.Text.Json.JsonSerializer.Serialize(_simpleOrder);

    [Benchmark]
    public string SimpleOrder_NewtonsoftJson() =>
        JsonConvert.SerializeObject(_simpleOrder);

    [Benchmark]
    public string SimpleOrder_TooNet() =>
        TooNetSerializer.Serialize(_simpleOrder);

    [Benchmark]
    public byte[] SimpleOrder_SystemTextJson_Utf8() =>
        System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(_simpleOrder);

    [Benchmark]
    public byte[] SimpleOrder_NewtonsoftJson_Utf8() =>
        Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(_simpleOrder));

    [Benchmark]
    public byte[] SimpleOrder_TooNet_Utf8() =>
        TooNetSerializer.SerializeToUtf8Bytes(_simpleOrder);

    [Benchmark]
    public string ComplexOrder_SystemTextJson() =>
        System.Text.Json.JsonSerializer.Serialize(_complexOrder);

    [Benchmark]
    public string ComplexOrder_NewtonsoftJson() =>
        JsonConvert.SerializeObject(_complexOrder);

    [Benchmark]
    public string ComplexOrder_TooNet() =>
        TooNetSerializer.Serialize(_complexOrder);

    [Benchmark]
    public byte[] ComplexOrder_SystemTextJson_Utf8() =>
        System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(_complexOrder);

    [Benchmark]
    public byte[] ComplexOrder_NewtonsoftJson_Utf8() =>
        Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(_complexOrder));

    [Benchmark]
    public byte[] ComplexOrder_TooNet_Utf8() =>
        TooNetSerializer.SerializeToUtf8Bytes(_complexOrder);

    [Benchmark]
    public string CustomerWithOrders_SystemTextJson()
    {
        var options = new JsonSerializerOptions
        {
            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
        };
        return System.Text.Json.JsonSerializer.Serialize(_customerWithOrders, options);
    }

    [Benchmark]
    public string CustomerWithOrders_NewtonsoftJson()
    {
        var settings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };
        return JsonConvert.SerializeObject(_customerWithOrders, settings);
    }

    [Benchmark]
    public string CustomerWithOrders_TooNet() =>
        TooNetSerializer.Serialize(_customerWithOrders);

    [Benchmark]
    public byte[] CustomerWithOrders_SystemTextJson_Utf8()
    {
        var options = new JsonSerializerOptions
        {
            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
        };
        return System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(_customerWithOrders, options);
    }

    [Benchmark]
    public byte[] CustomerWithOrders_NewtonsoftJson_Utf8()
    {
        var settings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };
        return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(_customerWithOrders, settings));
    }

    [Benchmark]
    public byte[] CustomerWithOrders_TooNet_Utf8() =>
        TooNetSerializer.SerializeToUtf8Bytes(_customerWithOrders);
}
