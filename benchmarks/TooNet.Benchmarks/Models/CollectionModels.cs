namespace TooNet.Benchmarks.Models;

public class Product
{
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string[] Tags { get; set; } = Array.Empty<string>();
}

public class ProductCatalog
{
    public string CatalogId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<Product> Products { get; set; } = new();
    public DateTime LastUpdated { get; set; }
}

public class MetricData
{
    public string MetricName { get; set; } = string.Empty;
    public double[] Values { get; set; } = Array.Empty<double>();
    public int[] Counts { get; set; } = Array.Empty<int>();
    public DateTime[] Timestamps { get; set; } = Array.Empty<DateTime>();
}

public class UserPreferences
{
    public string UserId { get; set; } = string.Empty;
    public Dictionary<string, object> Settings { get; set; } = new();
    public DateTime LastModified { get; set; }
}
