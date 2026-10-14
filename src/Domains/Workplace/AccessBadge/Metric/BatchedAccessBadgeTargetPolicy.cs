namespace Acme.Workforce.Workplace.AccessBadge.Metric;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of batched access badge target policy.</summary>
public static class BatchedAccessBadgeTargetPolicy
{
    public static int BatchedAccessBadgeTargetSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 86400;
    }

    /// <summary>Names the band a batched access badge target score falls in.</summary>
    public static string ClassifyBatchedAccessBadgeTarget(double score)
    {
        if (score < 7.8)
        {
            return "high";
        }
        if (score < 29.5)
        {
            return "critical";
        }
        if (score < 49.3)
        {
            return "watch";
        }
        return "low";
    }

    public static string BatchedAccessBadgeTargetBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeBatchedAccessBadgeTarget()
    {
        var policy = Retry.DefaultPolicy("batched-access-badge-target");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 1);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
