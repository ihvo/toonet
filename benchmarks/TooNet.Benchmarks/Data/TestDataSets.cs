namespace TooNet.Benchmarks.Data;

public static class TestDataSets
{
    // Lazy-loaded single objects
    private static readonly Lazy<SimplePrimitive> _smallObject = new(() => DataGenerator.GenerateSimplePrimitive());
    private static readonly Lazy<Order> _mediumObject = new(() => DataGenerator.GenerateOrder(5));
    private static readonly Lazy<Customer> _largeObject = new(() =>
    {
        var customer = new Customer
        {
            Id = Guid.NewGuid().ToString(),
            Name = StringGenerator.GenerateName(),
            Type = CustomerType.Premium,
            Addresses = new List<Address>
            {
                StringGenerator.GenerateAddress(),
                StringGenerator.GenerateAddress(),
                StringGenerator.GenerateAddress()
            }
        };
        customer.Email = StringGenerator.GenerateEmail(customer.Name);
        return customer;
    });

    public static SimplePrimitive SmallObject => _smallObject.Value;
    public static Order MediumObject => _mediumObject.Value;
    public static Customer LargeObject => _largeObject.Value;

    // Collection datasets
    public static List<SimplePrimitive> SmallCollection { get; } = DataGenerator.GenerateSimplePrimitives(10);
    public static List<SimplePrimitive> MediumCollection { get; } = DataGenerator.GenerateSimplePrimitives(100);
    public static List<SimplePrimitive> LargeCollection { get; } = DataGenerator.GenerateSimplePrimitives(1000);

    // Tabular datasets
    public static List<SalesRecord> TabularSmall { get; } = DataGenerator.GenerateSalesRecords(10);
    public static List<SalesRecord> TabularMedium { get; } = DataGenerator.GenerateSalesRecords(100);
    public static List<SalesRecord> TabularLarge { get; } = DataGenerator.GenerateSalesRecords(1000);

    // Additional datasets
    public static ProductCatalog SmallCatalog { get; } = DataGenerator.GenerateProductCatalog(10);
    public static ProductCatalog MediumCatalog { get; } = DataGenerator.GenerateProductCatalog(100);
    public static ProductCatalog LargeCatalog { get; } = DataGenerator.GenerateProductCatalog(1000);

    public static List<LogEntry> LogsSmall { get; } = DataGenerator.GenerateLogEntries(10);
    public static List<LogEntry> LogsMedium { get; } = DataGenerator.GenerateLogEntries(100);
    public static List<LogEntry> LogsLarge { get; } = DataGenerator.GenerateLogEntries(1000);

    public static List<StockPrice> StockPricesSmall { get; } = DataGenerator.GenerateStockPrices("AAPL", 10);
    public static List<StockPrice> StockPricesMedium { get; } = DataGenerator.GenerateStockPrices("AAPL", 100);
    public static List<StockPrice> StockPricesLarge { get; } = DataGenerator.GenerateStockPrices("AAPL", 1000);
}
