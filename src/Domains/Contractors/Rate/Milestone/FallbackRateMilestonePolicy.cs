namespace Acme.Workforce.Contractors.Rate.Milestone;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of fallback rate milestone policy.</summary>
public static class FallbackRateMilestonePolicy
{
    /// <summary>Names the band a fallback rate milestone score falls in.</summary>
    public static string ClassifyFallbackRateMilestone(double score)
    {
        if (score < 3.6)
        {
            return "high";
        }
        if (score < 25.3)
        {
            return "elevated";
        }
        if (score < 67.4)
        {
            return "low";
        }
        return "critical";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeFallbackRateMilestone()
    {
        var policy = Retry.DefaultPolicy("fallback-rate-milestone");
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
}
