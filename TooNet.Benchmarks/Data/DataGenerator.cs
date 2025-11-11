namespace TooNet.Benchmarks.Data;

public static class DataGenerator
{
    private static readonly Random _random = new(42);
    public static Random Random => _random;

    public static SimplePrimitive GenerateSimplePrimitive()
    {
        return new SimplePrimitive
        {
            Id = _random.Next(1, 10000),
            Name = StringGenerator.GenerateProductName(),
            IsActive = _random.Next(2) == 1,
            Value = _random.NextDouble() * 1000
        };
    }

    public static List<SimplePrimitive> GenerateSimplePrimitives(int count)
    {
        var list = new List<SimplePrimitive>(count);
        for (int i = 0; i < count; i++)
        {
            list.Add(GenerateSimplePrimitive());
        }
        return list;
    }

    public static ProductCatalog GenerateProductCatalog(int productCount)
    {
        var products = new List<Product>(productCount);
        for (int i = 0; i < productCount; i++)
        {
            products.Add(new Product
            {
                Sku = StringGenerator.GenerateSku(),
                Name = StringGenerator.GenerateProductName(),
                Price = (decimal)(_random.NextDouble() * 1000),
                Tags = GenerateTags(_random.Next(1, 5))
            });
        }

        return new ProductCatalog
        {
            CatalogId = Guid.NewGuid().ToString(),
            Name = "Product Catalog",
            Products = products,
            LastUpdated = DateTime.UtcNow
        };
    }

    public static Order GenerateOrder(int itemCount)
    {
        var customer = new Customer
        {
            Id = Guid.NewGuid().ToString(),
            Name = StringGenerator.GenerateName(),
            Email = "",
            Type = (CustomerType)_random.Next(3),
            Addresses = new List<Address> { StringGenerator.GenerateAddress() }
        };
        customer.Email = StringGenerator.GenerateEmail(customer.Name);

        var items = new List<OrderItem>(itemCount);
        decimal subtotal = 0;
        for (int i = 0; i < itemCount; i++)
        {
            var unitPrice = (decimal)(_random.NextDouble() * 100);
            var quantity = _random.Next(1, 10);
            var discount = (decimal)(_random.NextDouble() * 10);
            items.Add(new OrderItem
            {
                ProductId = StringGenerator.GenerateSku(),
                ProductName = StringGenerator.GenerateProductName(),
                Quantity = quantity,
                UnitPrice = unitPrice,
                Discount = discount
            });
            subtotal += unitPrice * quantity - discount;
        }

        var tax = subtotal * 0.08m;
        return new Order
        {
            OrderId = Guid.NewGuid().ToString(),
            Customer = customer,
            Items = items,
            ShippingAddress = StringGenerator.GenerateAddress(),
            BillingAddress = StringGenerator.GenerateAddress(),
            Subtotal = subtotal,
            Tax = tax,
            Total = subtotal + tax,
            Status = (OrderStatus)_random.Next(5),
            OrderDate = DateTime.UtcNow.AddDays(-_random.Next(30))
        };
    }

    public static List<SalesRecord> GenerateSalesRecords(int count)
    {
        var records = new List<SalesRecord>(count);
        for (int i = 0; i < count; i++)
        {
            var quantity = _random.Next(1, 100);
            records.Add(new SalesRecord
            {
                Date = DateTime.UtcNow.AddDays(-_random.Next(365)),
                ProductId = StringGenerator.GenerateSku(),
                Quantity = quantity,
                Revenue = (decimal)(_random.NextDouble() * 1000 * quantity)
            });
        }
        return records;
    }

    public static List<LogEntry> GenerateLogEntries(int count)
    {
        var categories = new[] { "System", "Application", "Database", "Network", "Security" };
        var sources = new[] { "API", "Worker", "Scheduler", null };
        var messages = new[] {
            "Operation completed successfully",
            "Request processed",
            "Connection established",
            "Data synchronization started",
            "Cache invalidated"
        };

        var entries = new List<LogEntry>(count);
        for (int i = 0; i < count; i++)
        {
            entries.Add(new LogEntry
            {
                Timestamp = DateTime.UtcNow.AddMinutes(-_random.Next(10000)),
                Level = (LogLevel)_random.Next(5),
                Category = categories[_random.Next(categories.Length)],
                Message = messages[_random.Next(messages.Length)],
                Source = sources[_random.Next(sources.Length)],
                ThreadId = _random.Next(1, 100)
            });
        }
        return entries;
    }

    public static List<StockPrice> GenerateStockPrices(string symbol, int days)
    {
        var prices = new List<StockPrice>(days);
        var basePrice = (decimal)(_random.NextDouble() * 100 + 50);

        for (int i = 0; i < days; i++)
        {
            var open = basePrice + (decimal)(_random.NextDouble() * 10 - 5);
            var close = open + (decimal)(_random.NextDouble() * 10 - 5);
            var high = Math.Max(open, close) + (decimal)(_random.NextDouble() * 5);
            var low = Math.Min(open, close) - (decimal)(_random.NextDouble() * 5);

            prices.Add(new StockPrice
            {
                Symbol = symbol,
                Date = DateTime.UtcNow.AddDays(-days + i),
                Open = open,
                High = high,
                Low = low,
                Close = close,
                Volume = (long)(_random.Next(1000000, 10000000))
            });

            basePrice = close;
        }
        return prices;
    }

    public static Order GenerateComplexOrder(int itemCount)
    {
        var customer = new Customer
        {
            Id = Guid.NewGuid().ToString(),
            Name = StringGenerator.GenerateName(),
            Email = "",
            Type = (CustomerType)_random.Next(3),
            Addresses = new List<Address>
            {
                StringGenerator.GenerateAddress(),
                StringGenerator.GenerateAddress(),
                StringGenerator.GenerateAddress()
            }
        };
        customer.Email = StringGenerator.GenerateEmail(customer.Name);

        var items = new List<OrderItem>(itemCount);
        decimal subtotal = 0;
        for (int i = 0; i < itemCount; i++)
        {
            var unitPrice = (decimal)(_random.NextDouble() * 100);
            var quantity = _random.Next(1, 10);
            var discount = (decimal)(_random.NextDouble() * 10);
            items.Add(new OrderItem
            {
                ProductId = StringGenerator.GenerateSku(),
                ProductName = StringGenerator.GenerateProductName(),
                Quantity = quantity,
                UnitPrice = unitPrice,
                Discount = discount
            });
            subtotal += unitPrice * quantity - discount;
        }

        var tax = subtotal * 0.08m;
        return new Order
        {
            OrderId = Guid.NewGuid().ToString(),
            Customer = customer,
            Items = items,
            ShippingAddress = StringGenerator.GenerateAddress(),
            BillingAddress = StringGenerator.GenerateAddress(),
            Subtotal = subtotal,
            Tax = tax,
            Total = subtotal + tax,
            Status = (OrderStatus)_random.Next(5),
            OrderDate = DateTime.UtcNow.AddDays(-_random.Next(30))
        };
    }

    public static object GenerateNestedStructure(int depth)
    {
        if (depth <= 0)
        {
            return new SimplePrimitive
            {
                Id = _random.Next(1, 10000),
                Name = StringGenerator.GenerateProductName(),
                IsActive = _random.Next(2) == 1,
                Value = _random.NextDouble() * 1000
            };
        }

        return new
        {
            Level = depth,
            Name = $"Level-{depth}",
            Value = _random.Next(1, 1000),
            Nested = GenerateNestedStructure(depth - 1)
        };
    }

    private static string[] GenerateTags(int count)
    {
        var tags = new[] { "electronics", "home", "office", "outdoor", "sports", "gaming", "tools", "accessories" };
        var result = new string[count];
        for (int i = 0; i < count; i++)
        {
            result[i] = tags[_random.Next(tags.Length)];
        }
        return result;
    }
}
