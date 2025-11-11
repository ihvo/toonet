namespace TooNet.Tests;

public class DelimiterTests
{
    [Fact]
    public void DelimiterHasCorrectCharValues()
    {
        // Assert
        Assert.Equal(',', (char)Delimiter.Comma);
        Assert.Equal('\t', (char)Delimiter.Tab);
        Assert.Equal('|', (char)Delimiter.Pipe);
    }

    [Fact]
    public void DelimiterHasCorrectIntValues()
    {
        // Assert
        Assert.Equal(44, (int)Delimiter.Comma);  // ASCII for ','
        Assert.Equal(9, (int)Delimiter.Tab);     // ASCII for '\t'
        Assert.Equal(124, (int)Delimiter.Pipe);  // ASCII for '|'
    }

    [Fact]
    public void DelimiterAllValuesAreDefined()
    {
        // Arrange
        var values = Enum.GetValues<Delimiter>();

        // Assert
        Assert.Equal(3, values.Length);
        Assert.Contains(Delimiter.Comma, values);
        Assert.Contains(Delimiter.Tab, values);
        Assert.Contains(Delimiter.Pipe, values);
    }
}