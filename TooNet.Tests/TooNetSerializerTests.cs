namespace TooNet.Tests;

public class TooNetSerializerTests
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool Active { get; set; }
    }

    public enum Status
    {
        Active,
        Inactive,
        Pending
    }

    public class UserWithStatus
    {
        public int Id { get; set; }
        public Status Status { get; set; }
    }

    [Fact]
    public void SerializeSimpleObject()
    {
        var user = new User
        {
            Id = 123,
            Name = "Ada",
            Active = true
        };

        var result = TooNetSerializer.Serialize(user);

        Assert.Contains("Id: 123", result);
        Assert.Contains("Name: Ada", result);
        Assert.Contains("Active: true", result);
    }

    [Fact]
    public void SerializePrimitives()
    {
        Assert.Equal("42", TooNetSerializer.Serialize(42));
        Assert.Equal("3.14", TooNetSerializer.Serialize(3.14));
        Assert.Equal("true", TooNetSerializer.Serialize(true));
        Assert.Equal("false", TooNetSerializer.Serialize(false));
        Assert.Equal("hello", TooNetSerializer.Serialize("hello"));
    }

    [Fact]
    public void SerializeNull()
    {
        string? nullString = null;
        var result = TooNetSerializer.Serialize(nullString);
        Assert.Equal("null", result);
    }

    [Fact]
    public void SerializeEnumAsString()
    {
        var user = new UserWithStatus
        {
            Id = 1,
            Status = Status.Active
        };

        var options = new TooNetSerializerOptions { WriteEnumsAsStrings = true };
        var result = TooNetSerializer.Serialize(user, options);

        Assert.Contains("Status: Active", result);
    }

    [Fact]
    public void SerializeEnumAsNumber()
    {
        var user = new UserWithStatus
        {
            Id = 1,
            Status = Status.Pending
        };

        var options = new TooNetSerializerOptions { WriteEnumsAsStrings = false };
        var result = TooNetSerializer.Serialize(user, options);

        Assert.Contains("Status: 2", result);
    }

    [Fact]
    public void SerializeIgnoreNullValues()
    {
        var obj = new { Name = "Test", Description = (string?)null };

        var options = new TooNetSerializerOptions { IgnoreNullValues = true };
        var result = TooNetSerializer.Serialize(obj, options);

        Assert.Contains("Name: Test", result);
        Assert.DoesNotContain("Description", result);
    }

    [Fact]
    public void SerializeIncludeNullValues()
    {
        var obj = new { Name = "Test", Description = (string?)null };

        var options = new TooNetSerializerOptions { IgnoreNullValues = false };
        var result = TooNetSerializer.Serialize(obj, options);

        Assert.Contains("Name: Test", result);
        Assert.Contains("Description: null", result);
    }

    [Fact]
    public void SerializeArray()
    {
        var numbers = new[] { 1, 2, 3, 4, 5 };
        var result = TooNetSerializer.Serialize(numbers);

        Assert.Equal("[5]: 1,2,3,4,5", result);
    }

    [Fact]
    public void SerializeStringArray()
    {
        var names = new[] { "Alice", "Bob", "Charlie" };
        var result = TooNetSerializer.Serialize(names);

        Assert.Equal("[3]: Alice,Bob,Charlie", result);
    }

    [Fact]
    public void SerializeBooleanArray()
    {
        var flags = new[] { true, false, true };
        var result = TooNetSerializer.Serialize(flags);

        Assert.Equal("[3]: true,false,true", result);
    }

    [Fact]
    public void SerializeEmptyArray()
    {
        var empty = Array.Empty<object>();
        var result = TooNetSerializer.Serialize(empty);

        Assert.Equal("[0]:", result);
    }

    [Fact]
    public void SerializeToUtf8BytesReturnsValidBytes()
    {
        var user = new User
        {
            Id = 42,
            Name = "Test",
            Active = true
        };

        var bytes = TooNetSerializer.SerializeToUtf8Bytes(user);
        var result = Encoding.UTF8.GetString(bytes);

        Assert.Contains("Id: 42", result);
        Assert.Contains("Name: Test", result);
        Assert.Contains("Active: true", result);
    }

    [Fact]
    public void SerializeNestedObject()
    {
        var obj = new
        {
            User = new User
            {
                Id = 1,
                Name = "Alice",
                Active = true
            }
        };

        var result = TooNetSerializer.Serialize(obj);

        Assert.Contains("User:", result);
        Assert.Contains("  Id: 1", result);
        Assert.Contains("  Name: Alice", result);
        Assert.Contains("  Active: true", result);
    }

    [Fact]
    public void SerializeObjectWithArray()
    {
        var obj = new
        {
            Name = "Test",
            Numbers = new[] { 1, 2, 3 }
        };

        var result = TooNetSerializer.Serialize(obj);

        Assert.Contains("Name: Test", result);
        Assert.Contains("Numbers[3]: 1,2,3", result);  // Array format is propertyName[count]: values
    }

    [Fact]
    public void SerializeMaxDepthExceededThrowsException()
    {
        var options = new TooNetSerializerOptions { MaxDepth = 2 };

        var deepObj = new
        {
            Level1 = new
            {
                Level2 = new
                {
                    Level3 = new
                    {
                        Value = "too deep"
                    }
                }
            }
        };

        Assert.Throws<TooNetException>(() => TooNetSerializer.Serialize(deepObj, options));
    }

    [Fact]
    public void SerializeWithCustomDelimiter()
    {
        var numbers = new[] { 1, 2, 3 };
        var options = new TooNetSerializerOptions { DefaultDelimiter = Delimiter.Pipe };
        var result = TooNetSerializer.Serialize(numbers, options);

        Assert.Equal("[3|]: 1|2|3", result);
    }

    [Fact]
    public void SerializeDecimalAsDouble()
    {
        var value = 123.456m;
        var result = TooNetSerializer.Serialize(value);

        Assert.Equal("123.456", result);
    }

    [Fact]
    public void SerializeFloat()
    {
        var value = 3.14f;
        var result = TooNetSerializer.Serialize(value);

        Assert.StartsWith("3.14", result);
    }

    [Fact]
    public void SerializeLong()
    {
        var value = 9999999999L;
        var result = TooNetSerializer.Serialize(value);

        Assert.Equal("9999999999", result);
    }
}
