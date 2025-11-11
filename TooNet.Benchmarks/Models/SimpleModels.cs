namespace TooNet.Benchmarks.Models;

public class SimplePrimitive
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public double Value { get; set; }
}

public class SimpleNested
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public SimplePrimitive? Details { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SimpleWithNulls
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public int? Age { get; set; }
    public double? Score { get; set; }
    public bool? IsVerified { get; set; }
}

public class SimpleWithDates
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
}
