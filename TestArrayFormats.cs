using TooNet;

// Test data
var primitives = new[] { 1, 2, 3, 4, 5 };
var mixedList = new object[] { "hello", 42, true, "world" };

var users = new[]
{
    new { Id = 1, Name = "Alice", Active = true },
    new { Id = 2, Name = "Bob", Active = false },
    new { Id = 3, Name = "Charlie", Active = true }
};

var nestedObjects = new[]
{
    new { Text = "Item 1", Details = new { X = 10, Y = 20 } },
    new { Text = "Item 2", Details = new { X = 30, Y = 40 } }
};

Console.WriteLine("=== Inline Format (primitives) ===");
var options1 = new TooNetSerializerOptions { ArrayMode = ArrayFormatMode.Inline };
Console.WriteLine(TooNetSerializer.Serialize(new { Items = primitives }, options1));
Console.WriteLine();

Console.WriteLine("=== Auto Format (primitives -> Inline) ===");
var options2 = new TooNetSerializerOptions { ArrayMode = ArrayFormatMode.Auto };
Console.WriteLine(TooNetSerializer.Serialize(new { Items = primitives }, options2));
Console.WriteLine();

Console.WriteLine("=== List Format (mixed primitives) ===");
var options3 = new TooNetSerializerOptions { ArrayMode = ArrayFormatMode.List };
Console.WriteLine(TooNetSerializer.Serialize(new { Items = mixedList }, options3));
Console.WriteLine();

Console.WriteLine("=== Auto Format (mixed -> List) ===");
Console.WriteLine(TooNetSerializer.Serialize(new { Items = mixedList }, options2));
Console.WriteLine();

Console.WriteLine("=== Tabular Format (uniform objects) ===");
var options4 = new TooNetSerializerOptions { ArrayMode = ArrayFormatMode.Tabular };
Console.WriteLine(TooNetSerializer.Serialize(new { Users = users }, options4));
Console.WriteLine();

Console.WriteLine("=== Auto Format (uniform objects -> Tabular) ===");
Console.WriteLine(TooNetSerializer.Serialize(new { Users = users }, options2));
Console.WriteLine();

Console.WriteLine("=== List Format with nested objects ===");
var options5 = new TooNetSerializerOptions { ArrayMode = ArrayFormatMode.List };
Console.WriteLine(TooNetSerializer.Serialize(new { Data = nestedObjects }, options5));
Console.WriteLine();

Console.WriteLine("=== Auto Format (nested objects -> List) ===");
Console.WriteLine(TooNetSerializer.Serialize(new { Data = nestedObjects }, options2));
