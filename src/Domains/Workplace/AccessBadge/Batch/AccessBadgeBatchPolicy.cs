namespace Acme.Workforce.Workplace.AccessBadge.Batch;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of access badge batch policy.</summary>
public static class AccessBadgeBatchPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAccessBadgeBatch()
    {
        var policy = Retry.DefaultPolicy("access-badge-batch");
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

    /// <summary>Names the band a access badge batch score falls in.</summary>
    public static string ClassifyAccessBadgeBatch(double score)
    {
        if (score < 8.8)
        {
            return "minimal";
        }
        if (score < 66.3)
        {
            return "high";
        }
        return "critical";
    }

    public static string AccessBadgeBatchBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }
}
