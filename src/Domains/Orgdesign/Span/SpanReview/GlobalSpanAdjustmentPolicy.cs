namespace Acme.Workforce.Orgdesign.Span.SpanReview;

/// <summary>Functions of global span adjustment policy.</summary>
public static class GlobalSpanAdjustmentPolicy
{
    public static int GlobalSpanAdjustmentSpan(int start, int end)
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
        return span / 86400;
    }

    /// <summary>Puts a global span adjustment code in its canonical GSA- form.</summary>
    public static string NormalizeGlobalSpanAdjustmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("GSA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"GSA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a global span adjustment score falls in.</summary>
    public static string ClassifyGlobalSpanAdjustment(double score)
    {
        if (score < 37.7)
        {
            return "elevated";
        }
        if (score < 46.9)
        {
            return "medium";
        }
        if (score < 76.9)
        {
            return "ok";
        }
        return "minimal";
    }
}
