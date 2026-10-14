namespace Acme.Workforce.Orgdesign.Span.Ledger;

/// <summary>Functions of span tier policy.</summary>
public static class SpanTierPolicy
{
    public static int SpanTierSpan(int start, int end)
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

    /// <summary>Puts a span tier code in its canonical ST- form.</summary>
    public static string NormalizeSpanTierCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("ST-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"ST-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
