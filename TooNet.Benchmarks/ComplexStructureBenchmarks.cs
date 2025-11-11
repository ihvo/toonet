namespace TooNet.Benchmarks;

[MemoryDiagnoser]
[JsonExporter]
public class ComplexStructureBenchmarks
{
    private Order _simpleOrder = null!;
    private Order _complexOrder = null!;
    private Customer _customerWithOrders = null!;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
    };

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
    public string SimpleOrder_TooNet() =>
        TooNetSerializer.Serialize(_simpleOrder);

    [Benchmark]
    public byte[] SimpleOrder_SystemTextJson_Utf8() =>
        System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(_simpleOrder);

    [Benchmark]
    public byte[] SimpleOrder_TooNet_Utf8() =>
        TooNetSerializer.SerializeToUtf8Bytes(_simpleOrder);

    [Benchmark]
    public string ComplexOrder_SystemTextJson() =>
        System.Text.Json.JsonSerializer.Serialize(_complexOrder);

    [Benchmark]
    public string ComplexOrder_TooNet() =>
        TooNetSerializer.Serialize(_complexOrder);

    [Benchmark]
    public byte[] ComplexOrder_SystemTextJson_Utf8() =>
        System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(_complexOrder);

    [Benchmark]
    public byte[] ComplexOrder_TooNet_Utf8() =>
        TooNetSerializer.SerializeToUtf8Bytes(_complexOrder);

    [Benchmark]
    public string CustomerWithOrders_SystemTextJson()
    {
        return System.Text.Json.JsonSerializer.Serialize(_customerWithOrders, JsonOptions);
    }

    [Benchmark]
    public string CustomerWithOrders_TooNet() =>
        TooNetSerializer.Serialize(_customerWithOrders);

    [Benchmark]
    public byte[] CustomerWithOrders_SystemTextJson_Utf8()
    {
        return System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(_customerWithOrders, JsonOptions);
    }

    [Benchmark]
    public byte[] CustomerWithOrders_TooNet_Utf8() =>
        TooNetSerializer.SerializeToUtf8Bytes(_customerWithOrders);
}
