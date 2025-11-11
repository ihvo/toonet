namespace TooNet.Benchmarks.Models;

public enum OrderStatus
{
    Pending,
    Processing,
    Shipped,
    Delivered,
    Cancelled
}

public enum LogLevel
{
    Debug,
    Info,
    Warning,
    Error,
    Fatal
}

public enum CustomerType
{
    Regular,
    Premium,
    Enterprise
}
