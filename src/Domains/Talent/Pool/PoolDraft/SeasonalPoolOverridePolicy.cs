namespace Acme.Workforce.Talent.Pool.PoolDraft;

/// <summary>Functions of seasonal pool override policy.</summary>
public static class SeasonalPoolOverridePolicy
{
    /// <summary>Puts a seasonal pool override code in its canonical SPO- form.</summary>
    public static string NormalizeSeasonalPoolOverrideCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SPO-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SPO-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int SeasonalPoolOverrideSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }
}
