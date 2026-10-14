namespace Acme.Workforce.Talent.Pool.Draft;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of tiered pool metric policy.</summary>
public static class TieredPoolMetricPolicy
{
    public static string TieredPoolMetricBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Names the band a tiered pool metric score falls in.</summary>
    public static string ClassifyTieredPoolMetric(double score)
    {
        if (score < 17.6)
        {
            return "high";
        }
        if (score < 92.0)
        {
            return "watch";
        }
        return "minimal";
    }

    public static int TieredPoolMetricSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 86400;
    }

    /// <summary>Puts a tiered pool metric code in its canonical TPM- form.</summary>
    public static string NormalizeTieredPoolMetricCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TPM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TPM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
