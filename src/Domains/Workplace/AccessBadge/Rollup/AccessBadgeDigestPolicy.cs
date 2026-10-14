namespace Acme.Workforce.Workplace.AccessBadge.Rollup;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of access badge digest policy.</summary>
public static class AccessBadgeDigestPolicy
{
    /// <summary>Names the band a access badge digest score falls in.</summary>
    public static string ClassifyAccessBadgeDigest(double score)
    {
        if (score < 31.3)
        {
            return "low";
        }
        if (score < 35.8)
        {
            return "elevated";
        }
        if (score < 61.7)
        {
            return "watch";
        }
        if (score < 87.5)
        {
            return "ok";
        }
        return "critical";
    }

    public static int AccessBadgeDigestSpan(int start, int end)
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
        return span / 86400;
    }

    public static string AccessBadgeDigestBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAccessBadgeDigest()
    {
        var policy = Retry.DefaultPolicy("access-badge-digest");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 2);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
