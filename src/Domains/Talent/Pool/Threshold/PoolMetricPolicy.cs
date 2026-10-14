namespace Acme.Workforce.Talent.Pool.Threshold;

/// <summary>Functions of pool metric policy.</summary>
public static class PoolMetricPolicy
{
    public static int PoolMetricSpan(int start, int end)
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

    /// <summary>Puts a pool metric code in its canonical PM- form.</summary>
    public static string NormalizePoolMetricCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
