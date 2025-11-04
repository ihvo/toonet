namespace TooNet.Benchmarks.Data;

public static class StringGenerator
{
    private static readonly string[] FirstNames = { "Alice", "Bob", "Charlie", "Diana", "Emma", "Frank", "Grace", "Henry", "Iris", "Jack" };
    private static readonly string[] LastNames = { "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", "Rodriguez", "Martinez" };
    private static readonly string[] ProductAdjectives = { "Premium", "Deluxe", "Basic", "Ultimate", "Professional", "Compact", "Advanced", "Classic", "Modern", "Elite" };
    private static readonly string[] ProductNouns = { "Widget", "Gadget", "Tool", "Device", "System", "Module", "Component", "Unit", "Adapter", "Controller" };
    private static readonly string[] Streets = { "Main St", "Oak Ave", "Maple Dr", "Pine Rd", "Cedar Ln", "Elm St", "Park Ave", "Lake Dr" };
    private static readonly string[] Cities = { "Springfield", "Portland", "Austin", "Denver", "Phoenix", "Seattle", "Boston", "Miami" };
    private static readonly string[] States = { "CA", "NY", "TX", "FL", "WA", "CO", "MA", "AZ" };
    private static readonly string[] Countries = { "USA", "Canada", "Mexico" };

    private static Random Random => DataGenerator.Random;

    public static string GenerateName()
    {
        var first = FirstNames[Random.Next(FirstNames.Length)];
        var last = LastNames[Random.Next(LastNames.Length)];
        return $"{first} {last}";
    }

    public static string GenerateEmail(string name)
    {
        var cleaned = name.ToLowerInvariant().Replace(" ", ".");
        return $"{cleaned}@example.com";
    }

    public static string GenerateProductName()
    {
        var adj = ProductAdjectives[Random.Next(ProductAdjectives.Length)];
        var noun = ProductNouns[Random.Next(ProductNouns.Length)];
        return $"{adj} {noun}";
    }

    public static string GenerateSku()
    {
        var prefix = (char)('A' + Random.Next(26));
        var number = Random.Next(10000, 99999);
        return $"{prefix}{number}";
    }

    public static Address GenerateAddress()
    {
        var streetNumber = Random.Next(100, 9999);
        return new Address
        {
            Street = $"{streetNumber} {Streets[Random.Next(Streets.Length)]}",
            City = Cities[Random.Next(Cities.Length)],
            State = States[Random.Next(States.Length)],
            PostalCode = Random.Next(10000, 99999).ToString(),
            Country = Countries[Random.Next(Countries.Length)]
        };
    }
}
