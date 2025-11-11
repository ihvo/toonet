namespace TooNet.Benchmarks.Utils;

public static class TokenCounter
{
    private const double CharsPerToken = 3.5;
    private const double JsonStructuralWeight = 1.2;
    private const double ToonStructuralWeight = 0.8;

    public static int EstimateTokens(string text)
    {
        if (string.IsNullOrEmpty(text)) return 0;

        var chars = text.Length;
        var whitespace = text.Count(char.IsWhiteSpace);
        var effectiveChars = chars - (whitespace * 0.5);

        return (int)Math.Ceiling(effectiveChars / CharsPerToken);
    }

    public static int EstimateJsonTokens(string json)
    {
        if (string.IsNullOrEmpty(json)) return 0;

        var structuralChars = json.Count(c => c is '{' or '}' or '[' or ']' or ':' or ',' or '"');
        var nonStructuralChars = json.Length - structuralChars;
        var whitespace = json.Count(char.IsWhiteSpace);

        var structuralTokens = structuralChars * JsonStructuralWeight;
        var contentTokens = (nonStructuralChars - whitespace) / CharsPerToken;

        return (int)Math.Ceiling(structuralTokens + contentTokens);
    }

    public static int EstimateToonTokens(string toon)
    {
        if (string.IsNullOrEmpty(toon)) return 0;

        var structuralChars = toon.Count(c => c is '|' or '[' or ']' or ':');
        var nonStructuralChars = toon.Length - structuralChars;
        var whitespace = toon.Count(char.IsWhiteSpace);

        var structuralTokens = structuralChars * ToonStructuralWeight;
        var contentTokens = (nonStructuralChars - whitespace) / CharsPerToken;

        return (int)Math.Ceiling(structuralTokens + contentTokens);
    }

    public static TokenComparison CompareFormats(string json, string toon)
    {
        var jsonTokens = EstimateJsonTokens(json);
        var toonTokens = EstimateToonTokens(toon);
        var saved = jsonTokens - toonTokens;
        var reduction = jsonTokens > 0 ? (saved / (double)jsonTokens) * 100 : 0;

        return new TokenComparison
        {
            JsonTokens = jsonTokens,
            ToonTokens = toonTokens,
            TokensSaved = saved,
            ReductionPercentage = reduction
        };
    }
}

public class TokenComparison
{
    public int JsonTokens { get; set; }
    public int ToonTokens { get; set; }
    public double ReductionPercentage { get; set; }
    public int TokensSaved { get; set; }
}
