namespace TooNet.Benchmarks.Reporting;

public static class BenchmarkReporter
{
    public static void PrintTokenComparison(string title, string json, string toon)
    {
        Console.WriteLine($"\n{title}");
        Console.WriteLine(new string('=', title.Length));

        var comparison = TokenCounter.CompareFormats(json, toon);
        var jsonBytes = Encoding.UTF8.GetByteCount(json);
        var toonBytes = Encoding.UTF8.GetByteCount(toon);

        Console.WriteLine($"JSON:  {jsonBytes,8:N0} bytes ({comparison.JsonTokens,5:N0} tokens)");
        Console.WriteLine($"TOON:  {toonBytes,8:N0} bytes ({comparison.ToonTokens,5:N0} tokens)");
        Console.WriteLine($"Saved: {jsonBytes - toonBytes,8:N0} bytes ({comparison.TokensSaved,5:N0} tokens)");
        Console.WriteLine($"Reduction: {comparison.ReductionPercentage:F1}%");
    }

    public static void GenerateMarkdownReport(List<TokenReductionAnalyzer.ReductionResult> results, string outputPath)
    {
        var md = new StringBuilder();
        md.AppendLine("# TOON Format Benchmark Results");
        md.AppendLine();
        md.AppendLine("## Token Reduction Analysis");
        md.AppendLine();
        md.AppendLine("| Dataset | JSON Bytes | JSON Tokens | TOON Bytes | TOON Tokens | Byte Reduction | Token Reduction |");
        md.AppendLine("|---------|------------|-------------|------------|-------------|----------------|-----------------|");

        foreach (var result in results)
        {
            md.AppendLine($"| {result.DatasetName} | {result.JsonBytes:N0} | {result.JsonTokens:N0} | " +
                         $"{result.ToonBytes:N0} | {result.ToonTokens:N0} | " +
                         $"{result.ByteReduction:F1}% | {result.TokenReduction:F1}% |");
        }

        md.AppendLine();
        md.AppendLine("## Summary");
        md.AppendLine();
        var avgByteReduction = results.Average(r => r.ByteReduction);
        var avgTokenReduction = results.Average(r => r.TokenReduction);
        md.AppendLine($"- **Average Byte Reduction**: {avgByteReduction:F1}%");
        md.AppendLine($"- **Average Token Reduction**: {avgTokenReduction:F1}%");
        md.AppendLine($"- **Total JSON Bytes**: {results.Sum(r => r.JsonBytes):N0}");
        md.AppendLine($"- **Total TOON Bytes**: {results.Sum(r => r.ToonBytes):N0}");
        md.AppendLine($"- **Total Bytes Saved**: {results.Sum(r => r.JsonBytes - r.ToonBytes):N0}");

        File.WriteAllText(outputPath, md.ToString());
        Console.WriteLine($"Markdown report generated: {outputPath}");
    }

    public static void GenerateHtmlReport(List<TokenReductionAnalyzer.ReductionResult> results, string outputPath)
    {
        var html = new StringBuilder();
        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html>");
        html.AppendLine("<head>");
        html.AppendLine("    <title>TOON Format Benchmark Results</title>");
        html.AppendLine("    <style>");
        html.AppendLine("        body { font-family: Arial, sans-serif; margin: 20px; }");
        html.AppendLine("        h1 { color: #333; }");
        html.AppendLine("        table { border-collapse: collapse; width: 100%; margin: 20px 0; }");
        html.AppendLine("        th, td { border: 1px solid #ddd; padding: 12px; text-align: left; }");
        html.AppendLine("        th { background-color: #4CAF50; color: white; }");
        html.AppendLine("        tr:nth-child(even) { background-color: #f2f2f2; }");
        html.AppendLine("        .summary { background-color: #e7f3e7; padding: 15px; border-radius: 5px; margin: 20px 0; }");
        html.AppendLine("        .number { text-align: right; }");
        html.AppendLine("    </style>");
        html.AppendLine("</head>");
        html.AppendLine("<body>");
        html.AppendLine("    <h1>TOON Format Benchmark Results</h1>");
        html.AppendLine("    <h2>Token Reduction Analysis</h2>");
        html.AppendLine("    <table>");
        html.AppendLine("        <tr>");
        html.AppendLine("            <th>Dataset</th>");
        html.AppendLine("            <th class='number'>JSON Bytes</th>");
        html.AppendLine("            <th class='number'>JSON Tokens</th>");
        html.AppendLine("            <th class='number'>TOON Bytes</th>");
        html.AppendLine("            <th class='number'>TOON Tokens</th>");
        html.AppendLine("            <th class='number'>Byte Reduction</th>");
        html.AppendLine("            <th class='number'>Token Reduction</th>");
        html.AppendLine("        </tr>");

        foreach (var result in results)
        {
            html.AppendLine("        <tr>");
            html.AppendLine($"            <td>{result.DatasetName}</td>");
            html.AppendLine($"            <td class='number'>{result.JsonBytes:N0}</td>");
            html.AppendLine($"            <td class='number'>{result.JsonTokens:N0}</td>");
            html.AppendLine($"            <td class='number'>{result.ToonBytes:N0}</td>");
            html.AppendLine($"            <td class='number'>{result.ToonTokens:N0}</td>");
            html.AppendLine($"            <td class='number'>{result.ByteReduction:F1}%</td>");
            html.AppendLine($"            <td class='number'>{result.TokenReduction:F1}%</td>");
            html.AppendLine("        </tr>");
        }

        html.AppendLine("    </table>");
        html.AppendLine("    <div class='summary'>");
        html.AppendLine("        <h2>Summary</h2>");
        html.AppendLine($"        <p><strong>Average Byte Reduction:</strong> {results.Average(r => r.ByteReduction):F1}%</p>");
        html.AppendLine($"        <p><strong>Average Token Reduction:</strong> {results.Average(r => r.TokenReduction):F1}%</p>");
        html.AppendLine($"        <p><strong>Total JSON Bytes:</strong> {results.Sum(r => r.JsonBytes):N0}</p>");
        html.AppendLine($"        <p><strong>Total TOON Bytes:</strong> {results.Sum(r => r.ToonBytes):N0}</p>");
        html.AppendLine($"        <p><strong>Total Bytes Saved:</strong> {results.Sum(r => r.JsonBytes - r.ToonBytes):N0}</p>");
        html.AppendLine("    </div>");
        html.AppendLine("</body>");
        html.AppendLine("</html>");

        File.WriteAllText(outputPath, html.ToString());
        Console.WriteLine($"HTML report generated: {outputPath}");
    }
}
