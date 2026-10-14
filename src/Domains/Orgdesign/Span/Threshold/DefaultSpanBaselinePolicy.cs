namespace Acme.Workforce.Orgdesign.Span.Threshold;

/// <summary>Functions of default span baseline policy.</summary>
public static class DefaultSpanBaselinePolicy
{
    public static int DefaultSpanBaselineSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 60;
    }

    /// <summary>Puts a default span baseline code in its canonical DSB- form.</summary>
    public static string NormalizeDefaultSpanBaselineCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("DSB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"DSB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a default span baseline score falls in.</summary>
    public static string ClassifyDefaultSpanBaseline(double score)
    {
        if (score < 24.2)
        {
            return "critical";
        }
        if (score < 32.3)
        {
            return "watch";
        }
        return "high";
    }
}
