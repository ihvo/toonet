namespace TooNet.Benchmarks.Utils;

public static class TokenAnalyzer
{
    public static TokenBreakdown AnalyzeContent(string content)
    {
        if (string.IsNullOrEmpty(content))
            return new TokenBreakdown();

        var structural = content.Count(c => c is '{' or '}' or '[' or ']' or ':' or ',' or '"' or '|');
        var whitespace = content.Count(char.IsWhiteSpace);

        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var keyChars = 0;

        foreach (var line in lines)
        {
            var colonIndex = line.IndexOf(':');
            if (colonIndex > 0)
            {
                var beforeColon = line[..colonIndex].Trim();
                keyChars += beforeColon.Length;
            }
        }

        var valueChars = content.Length - structural - whitespace - keyChars;

        return new TokenBreakdown
        {
            StructuralTokens = (int)(structural * 1.1),
            KeyTokens = (int)Math.Ceiling(keyChars / 3.5),
            ValueTokens = (int)Math.Ceiling(valueChars / 3.5),
            WhitespaceTokens = (int)(whitespace * 0.1),
            TotalTokens = TokenCounter.EstimateTokens(content)
        };
    }

    public static EfficiencyReport CompareEfficiency(string json, string toon)
    {
        var jsonBreakdown = AnalyzeContent(json);
        var toonBreakdown = AnalyzeContent(toon);

        var savings = new Dictionary<string, int>
        {
            ["Structural"] = jsonBreakdown.StructuralTokens - toonBreakdown.StructuralTokens,
            ["Keys"] = jsonBreakdown.KeyTokens - toonBreakdown.KeyTokens,
            ["Values"] = jsonBreakdown.ValueTokens - toonBreakdown.ValueTokens,
            ["Whitespace"] = jsonBreakdown.WhitespaceTokens - toonBreakdown.WhitespaceTokens
        };

        return new EfficiencyReport
        {
            JsonBreakdown = jsonBreakdown,
            ToonBreakdown = toonBreakdown,
            SavingsByCategory = savings
        };
    }
}

public class TokenBreakdown
{
    public int StructuralTokens { get; set; }
    public int KeyTokens { get; set; }
    public int ValueTokens { get; set; }
    public int WhitespaceTokens { get; set; }
    public int TotalTokens { get; set; }
}

public class EfficiencyReport
{
    public TokenBreakdown JsonBreakdown { get; set; } = new();
    public TokenBreakdown ToonBreakdown { get; set; } = new();
    public Dictionary<string, int> SavingsByCategory { get; set; } = new();
}
