namespace Acme.Workforce.Orgdesign.Span.Estimate;

/// <summary>Functions of span group policy.</summary>
public static class SpanGroupPolicy
{
    public static int SpanGroupSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 3600;
    }

    /// <summary>Puts a span group code in its canonical SG- form.</summary>
    public static string NormalizeSpanGroupCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SG-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SG-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a span group score falls in.</summary>
    public static string ClassifySpanGroup(double score)
    {
        if (score < 6.5)
        {
            return "critical";
        }
        if (score < 44.1)
        {
            return "ok";
        }
        if (score < 72.2)
        {
            return "elevated";
        }
        if (score < 81.8)
        {
            return "high";
        }
        return "watch";
    }
}
