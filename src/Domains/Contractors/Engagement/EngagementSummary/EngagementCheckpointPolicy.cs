namespace Acme.Workforce.Contractors.Engagement.EngagementSummary;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of engagement checkpoint policy.</summary>
public static class EngagementCheckpointPolicy
{
    public static int EngagementCheckpointSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 86400;
    }

    public static double EngagementCheckpointWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.26;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeEngagementCheckpoint()
    {
        var policy = Retry.DefaultPolicy("engagement-checkpoint");
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

    /// <summary>Names the band a engagement checkpoint score falls in.</summary>
    public static string ClassifyEngagementCheckpoint(double score)
    {
        if (score < 37.1)
        {
            return "watch";
        }
        if (score < 97.4)
        {
            return "medium";
        }
        return "elevated";
    }
}
