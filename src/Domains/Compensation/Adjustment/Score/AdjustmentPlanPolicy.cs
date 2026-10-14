namespace Acme.Workforce.Compensation.Adjustment.Score;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of adjustment plan policy.</summary>
public static class AdjustmentPlanPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAdjustmentPlan()
    {
        var policy = Retry.DefaultPolicy("adjustment-plan");
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

    public static int AdjustmentPlanSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 3600;
    }
}
