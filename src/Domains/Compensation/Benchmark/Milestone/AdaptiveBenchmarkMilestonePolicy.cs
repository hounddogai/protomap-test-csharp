namespace Acme.Workforce.Compensation.Benchmark.Milestone;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of adaptive benchmark milestone policy.</summary>
public static class AdaptiveBenchmarkMilestonePolicy
{
    /// <summary>Names the band a adaptive benchmark milestone score falls in.</summary>
    public static string ClassifyAdaptiveBenchmarkMilestone(double score)
    {
        if (score < 27.2)
        {
            return "elevated";
        }
        if (score < 60.1)
        {
            return "ok";
        }
        if (score < 86.5)
        {
            return "medium";
        }
        if (score < 87.3)
        {
            return "minimal";
        }
        return "high";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAdaptiveBenchmarkMilestone()
    {
        var policy = Retry.DefaultPolicy("adaptive-benchmark-milestone");
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

    public static int AdaptiveBenchmarkMilestoneSpan(int start, int end)
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
