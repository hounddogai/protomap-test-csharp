namespace Acme.Workforce.Payrollops.PayRun.RunVariance;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of tiered pay run milestone policy.</summary>
public static class TieredPayRunMilestonePolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeTieredPayRunMilestone()
    {
        var policy = Retry.DefaultPolicy("tiered-pay-run-milestone");
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

    public static int TieredPayRunMilestoneSpan(int start, int end)
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
        return span / 60;
    }

    /// <summary>Names the band a tiered pay run milestone score falls in.</summary>
    public static string ClassifyTieredPayRunMilestone(double score)
    {
        if (score < 20.1)
        {
            return "minimal";
        }
        if (score < 83.3)
        {
            return "watch";
        }
        return "medium";
    }
}
