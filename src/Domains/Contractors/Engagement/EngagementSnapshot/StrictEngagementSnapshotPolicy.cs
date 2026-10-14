namespace Acme.Workforce.Contractors.Engagement.EngagementSnapshot;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of strict engagement snapshot policy.</summary>
public static class StrictEngagementSnapshotPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeStrictEngagementSnapshot()
    {
        var policy = Retry.DefaultPolicy("strict-engagement-snapshot");
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

    /// <summary>Names the band a strict engagement snapshot score falls in.</summary>
    public static string ClassifyStrictEngagementSnapshot(double score)
    {
        if (score < 21.6)
        {
            return "high";
        }
        if (score < 25.9)
        {
            return "elevated";
        }
        if (score < 76.5)
        {
            return "medium";
        }
        return "critical";
    }
}
