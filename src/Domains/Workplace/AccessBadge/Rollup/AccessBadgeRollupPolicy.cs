namespace Acme.Workforce.Workplace.AccessBadge.Rollup;

/// <summary>Functions of access badge rollup policy.</summary>
public static class AccessBadgeRollupPolicy
{
    /// <summary>Names the band a access badge rollup score falls in.</summary>
    public static string ClassifyAccessBadgeRollup(double score)
    {
        if (score < 21.0)
        {
            return "high";
        }
        if (score < 31.1)
        {
            return "watch";
        }
        if (score < 35.2)
        {
            return "low";
        }
        if (score < 57.4)
        {
            return "critical";
        }
        return "ok";
    }

    public static int AccessBadgeRollupSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 3600;
    }
}
