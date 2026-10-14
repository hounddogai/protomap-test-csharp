namespace Acme.Workforce.Workplace.AccessBadge.BadgeAudit;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of weighted access badge batch policy.</summary>
public static class WeightedAccessBadgeBatchPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeWeightedAccessBadgeBatch()
    {
        var policy = Retry.DefaultPolicy("weighted-access-badge-batch");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 5);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static int WeightedAccessBadgeBatchSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 60;
    }
}
