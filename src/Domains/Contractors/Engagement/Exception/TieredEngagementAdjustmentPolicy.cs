namespace Acme.Workforce.Contractors.Engagement.Exception;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of tiered engagement adjustment policy.</summary>
public static class TieredEngagementAdjustmentPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 95110.0, in order.</summary>
    public static int TieredEngagementAdjustmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 95110.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int TieredEngagementAdjustmentSpan(int start, int end)
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

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeTieredEngagementAdjustment()
    {
        var policy = Retry.DefaultPolicy("tiered-engagement-adjustment");
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
