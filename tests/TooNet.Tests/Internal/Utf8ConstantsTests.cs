namespace TooNet.Tests.Internal;

public class Utf8ConstantsTests
{
    [Fact]
    public void TrueReturnsCorrectBytes()
    {
        // Arrange
        var expected = "true"u8.ToArray();

        // Act
        var actual = Utf8Constants.True.ToArray();

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void FalseReturnsCorrectBytes()
    {
        // Arrange
        var expected = "false"u8.ToArray();

        // Act
        var actual = Utf8Constants.False.ToArray();

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void NullReturnsCorrectBytes()
    {
        // Arrange
        var expected = "null"u8.ToArray();

        // Act
        var actual = Utf8Constants.Null.ToArray();

        // Assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(Utf8Constants.Quote, '"')]
    [InlineData(Utf8Constants.Backslash, '\\')]
    [InlineData(Utf8Constants.Colon, ':')]
    [InlineData(Utf8Constants.Space, ' ')]
    [InlineData(Utf8Constants.Newline, '\n')]
    [InlineData(Utf8Constants.CarriageReturn, '\r')]
    [InlineData(Utf8Constants.Tab, '\t')]
    [InlineData(Utf8Constants.Comma, ',')]
    [InlineData(Utf8Constants.Pipe, '|')]
    [InlineData(Utf8Constants.OpenBracket, '[')]
    [InlineData(Utf8Constants.CloseBracket, ']')]
    [InlineData(Utf8Constants.OpenBrace, '{')]
    [InlineData(Utf8Constants.CloseBrace, '}')]
    [InlineData(Utf8Constants.Hyphen, '-')]
    [InlineData(Utf8Constants.Hash, '#')]
    public void ByteConstantsHaveCorrectValues(byte actual, char expected)
    {
        // Assert
        Assert.Equal((byte)expected, actual);
    }

    [Theory]
    [InlineData(Utf8Constants.LowerN, 'n')]
    [InlineData(Utf8Constants.LowerR, 'r')]
    [InlineData(Utf8Constants.LowerT, 't')]
    public void EscapeCharacterBytesHaveCorrectValues(byte actual, char expected)
    {
        // Assert
        Assert.Equal((byte)expected, actual);
    }

    [Fact]
    public void AllConstantsAreValidAscii()
    {
        // Arrange
        byte[] allBytes =
        {
            Utf8Constants.Quote,
            Utf8Constants.Backslash,
            Utf8Constants.Colon,
            Utf8Constants.Space,
            Utf8Constants.Newline,
            Utf8Constants.CarriageReturn,
            Utf8Constants.Tab,
            Utf8Constants.Comma,
            Utf8Constants.Pipe,
            Utf8Constants.OpenBracket,
            Utf8Constants.CloseBracket,
            Utf8Constants.OpenBrace,
            Utf8Constants.CloseBrace,
            Utf8Constants.Hyphen,
            Utf8Constants.Hash,
            Utf8Constants.LowerN,
            Utf8Constants.LowerR,
            Utf8Constants.LowerT
        };

        // Assert - all should be valid ASCII (< 128)
        foreach (var b in allBytes)
        {
            Assert.True(b < 128, $"Byte value {b} is not valid ASCII");
        }
    }
}