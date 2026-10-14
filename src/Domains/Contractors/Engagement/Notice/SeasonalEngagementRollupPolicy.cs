namespace Acme.Workforce.Contractors.Engagement.Notice;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of seasonal engagement rollup policy.</summary>
public static class SeasonalEngagementRollupPolicy
{
    /// <summary>Puts a seasonal engagement rollup code in its canonical SER- form.</summary>
    public static string NormalizeSeasonalEngagementRollupCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SER-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SER-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeSeasonalEngagementRollup()
    {
        var policy = Retry.DefaultPolicy("seasonal-engagement-rollup");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 3);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
