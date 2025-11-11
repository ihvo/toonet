using Xunit;

namespace TooNet.Tests;

public class ArrayFormatTests
{
    [Fact]
    public void InlineFormat_PrimitiveArray_SerializesCorrectly()
    {
        var data = new { Items = new[] { 1, 2, 3, 4, 5 } };
        var options = new TooNetSerializerOptions { ArrayMode = ArrayFormatMode.Inline };

        var result = TooNetSerializer.Serialize(data, options);

        Assert.Equal("Items[5]: 1,2,3,4,5", result);
    }

    [Fact]
    public void InlineFormat_StringArray_SerializesCorrectly()
    {
        var data = new { Tags = new[] { "reading", "gaming", "coding" } };
        var options = new TooNetSerializerOptions { ArrayMode = ArrayFormatMode.Inline };

        var result = TooNetSerializer.Serialize(data, options);

        Assert.Equal("Tags[3]: reading,gaming,coding", result);
    }

    [Fact]
    public void ListFormat_PrimitiveArray_SerializesCorrectly()
    {
        var data = new { Items = new[] { 1, 2, 3 } };
        var options = new TooNetSerializerOptions { ArrayMode = ArrayFormatMode.List };

        var result = TooNetSerializer.Serialize(data, options);

        var expected = @"Items[3]:
  - 1
  - 2
  - 3";
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ListFormat_MixedPrimitives_SerializesCorrectly()
    {
        var data = new { Items = new object[] { "hello", 42, true, "world" } };
        var options = new TooNetSerializerOptions { ArrayMode = ArrayFormatMode.List };

        var result = TooNetSerializer.Serialize(data, options);

        var expected = @"Items[4]:
  - hello
  - 42
  - true
  - world";
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ListFormat_WithNestedObjects_SerializesCorrectly()
    {
        var data = new
        {
            Items = new[]
            {
                new { Id = 1, Name = "First" },
                new { Id = 2, Name = "Second" }
            }
        };
        var options = new TooNetSerializerOptions { ArrayMode = ArrayFormatMode.List };

        var result = TooNetSerializer.Serialize(data, options);

        var expected = @"Items[2]:
  - Id: 1
    Name: First
  - Id: 2
    Name: Second";
        Assert.Equal(expected, result);
    }

    [Fact]
    public void TabularFormat_UniformObjects_SerializesCorrectly()
    {
        var data = new
        {
            Users = new[]
            {
                new { Id = 1, Name = "Alice", Active = true },
                new { Id = 2, Name = "Bob", Active = false },
                new { Id = 3, Name = "Charlie", Active = true }
            }
        };
        var options = new TooNetSerializerOptions { ArrayMode = ArrayFormatMode.Tabular };

        var result = TooNetSerializer.Serialize(data, options);

        var expected = @"Users[3]{Id,Name,Active}:
  1,Alice,true
  2,Bob,false
  3,Charlie,true";
        Assert.Equal(expected, result);
    }

    [Fact]
    public void TabularFormat_WithNullValues_SerializesCorrectly()
    {
        var items = new[]
        {
            new { Id = 1, Name = (string?)"Alice" },
            new { Id = 2, Name = (string?)null },
            new { Id = 3, Name = (string?)"Charlie" }
        };
        var data = new { Items = items };
        var options = new TooNetSerializerOptions { ArrayMode = ArrayFormatMode.Tabular };

        var result = TooNetSerializer.Serialize(data, options);

        var expected = @"Items[3]{Id,Name}:
  1,Alice
  2,null
  3,Charlie";
        Assert.Equal(expected, result);
    }

    [Fact]
    public void AutoFormat_PrimitiveArray_UsesInline()
    {
        var data = new { Items = new[] { 1, 2, 3, 4, 5 } };
        var options = new TooNetSerializerOptions { ArrayMode = ArrayFormatMode.Auto };

        var result = TooNetSerializer.Serialize(data, options);

        Assert.Equal("Items[5]: 1,2,3,4,5", result);
    }

    [Fact]
    public void AutoFormat_UniformObjectsAboveThreshold_UsesTabular()
    {
        var data = new
        {
            Users = new[]
            {
                new { Id = 1, Name = "Alice" },
                new { Id = 2, Name = "Bob" },
                new { Id = 3, Name = "Charlie" }
            }
        };
        var options = new TooNetSerializerOptions
        {
            ArrayMode = ArrayFormatMode.Auto,
            TabularThreshold = 3
        };

        var result = TooNetSerializer.Serialize(data, options);

        var expected = @"Users[3]{Id,Name}:
  1,Alice
  2,Bob
  3,Charlie";
        Assert.Equal(expected, result);
    }

    [Fact]
    public void AutoFormat_UniformObjectsBelowThreshold_UsesList()
    {
        var data = new
        {
            Users = new[]
            {
                new { Id = 1, Name = "Alice" },
                new { Id = 2, Name = "Bob" }
            }
        };
        var options = new TooNetSerializerOptions
        {
            ArrayMode = ArrayFormatMode.Auto,
            TabularThreshold = 3
        };

        var result = TooNetSerializer.Serialize(data, options);

        var expected = @"Users[2]:
  - Id: 1
    Name: Alice
  - Id: 2
    Name: Bob";
        Assert.Equal(expected, result);
    }

    [Fact]
    public void AutoFormat_NestedObjects_UsesList()
    {
        var data = new
        {
            Items = new[]
            {
                new { Text = "Item 1", Details = new { X = 10 } },
                new { Text = "Item 2", Details = new { X = 20 } }
            }
        };
        var options = new TooNetSerializerOptions { ArrayMode = ArrayFormatMode.Auto };

        var result = TooNetSerializer.Serialize(data, options);

        // Should use List format because Details is not a primitive
        Assert.StartsWith("Items[2]:\n  - ", result);
    }

    [Fact]
    public void TabularFormat_WithDelimiter_UsesCorrectDelimiter()
    {
        var data = new
        {
            Users = new[]
            {
                new { Id = 1, Name = "Alice" },
                new { Id = 2, Name = "Bob" }
            }
        };
        var options = new TooNetSerializerOptions
        {
            ArrayMode = ArrayFormatMode.Tabular,
            DefaultDelimiter = Delimiter.Pipe
        };

        var result = TooNetSerializer.Serialize(data, options);

        var expected = @"Users[2|]{Id|Name}:
  1|Alice
  2|Bob";
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ListFormat_WithNulls_SerializesCorrectly()
    {
        var data = new { Items = new int?[] { 1, null, 3 } };
        var options = new TooNetSerializerOptions { ArrayMode = ArrayFormatMode.List };

        var result = TooNetSerializer.Serialize(data, options);

        var expected = @"Items[3]:
  - 1
  - null
  - 3";
        Assert.Equal(expected, result);
    }

    [Fact]
    public void TabularFormat_EmptyArray_SerializesCorrectly()
    {
        var data = new { Items = Array.Empty<object>() };
        var options = new TooNetSerializerOptions { ArrayMode = ArrayFormatMode.Tabular };

        var result = TooNetSerializer.Serialize(data, options);

        Assert.Equal("Items[0]:", result);
    }

    [Fact]
    public void ListFormat_EmptyArray_SerializesCorrectly()
    {
        var data = new { Items = Array.Empty<int>() };
        var options = new TooNetSerializerOptions { ArrayMode = ArrayFormatMode.List };

        var result = TooNetSerializer.Serialize(data, options);

        Assert.Equal("Items[0]:", result);
    }

    [Fact]
    public void TabularFormat_NonUniformObjects_FallsBackToList()
    {
        var data = new
        {
            Items = new object[]
            {
                new { Id = 1, Name = "Alice" },
                new { Id = 2, Type = "Admin" } // Different properties
            }
        };
        var options = new TooNetSerializerOptions { ArrayMode = ArrayFormatMode.Tabular };

        var result = TooNetSerializer.Serialize(data, options);

        // Should fall back to List format
        Assert.StartsWith("Items[2]:\n  - ", result);
    }

    [Fact]
    public void TabularFormat_WithDoubleValues_SerializesCorrectly()
    {
        var data = new
        {
            Points = new[]
            {
                new { X = 1.5, Y = 2.7 },
                new { X = 3.14, Y = 4.2 }
            }
        };
        var options = new TooNetSerializerOptions { ArrayMode = ArrayFormatMode.Tabular };

        var result = TooNetSerializer.Serialize(data, options);

        Assert.Contains("1.5", result);
        Assert.Contains("2.7", result);
        Assert.Contains("3.14", result);
    }

    [Fact]
    public void ListFormat_NestedObjectsWithMultipleLevels_SerializesCorrectly()
    {
        var data = new
        {
            Items = new[]
            {
                new
                {
                    Id = 1,
                    Child = new { Name = "ChildA", Value = 10 }
                },
                new
                {
                    Id = 2,
                    Child = new { Name = "ChildB", Value = 20 }
                }
            }
        };
        var options = new TooNetSerializerOptions { ArrayMode = ArrayFormatMode.List };

        var result = TooNetSerializer.Serialize(data, options);

        Assert.Contains("- Id: 1", result);
        Assert.Contains("Child:", result);
        Assert.Contains("Name: ChildA", result);
    }

    [Fact]
    public void TabularFormat_WithEnums_SerializesCorrectly()
    {
        var data = new
        {
            Users = new[]
            {
                new { Id = 1, Status = UserStatus.Active },
                new { Id = 2, Status = UserStatus.Inactive }
            }
        };
        var options = new TooNetSerializerOptions
        {
            ArrayMode = ArrayFormatMode.Tabular,
            WriteEnumsAsStrings = true
        };

        var result = TooNetSerializer.Serialize(data, options);

        var expected = @"Users[2]{Id,Status}:
  1,Active
  2,Inactive";
        Assert.Equal(expected, result);
    }

    private enum UserStatus
    {
        Active,
        Inactive
    }
}
