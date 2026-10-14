namespace Acme.Workforce.Workplace.AccessBadge.Audit;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of access badge threshold policy.</summary>
public static class AccessBadgeThresholdPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAccessBadgeThreshold()
    {
        var policy = Retry.DefaultPolicy("access-badge-threshold");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 4);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Names the band a access badge threshold score falls in.</summary>
    public static string ClassifyAccessBadgeThreshold(double score)
    {
        if (score < 14.2)
        {
            return "watch";
        }
        if (score < 15.5)
        {
            return "critical";
        }
        if (score < 51.5)
        {
            return "high";
        }
        return "elevated";
    }

    public static int AccessBadgeThresholdSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 60;
    }
}
