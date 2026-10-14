namespace Acme.Workforce.Talent.SuccessionPlan.Score;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of strict succession plan override policy.</summary>
public static class StrictSuccessionPlanOverridePolicy
{
    public static int StrictSuccessionPlanOverrideSpan(int start, int end)
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

    /// <summary>Counts how many amounts fit in a budget of 89794.0, in order.</summary>
    public static int StrictSuccessionPlanOverrideBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 89794.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeStrictSuccessionPlanOverride()
    {
        var policy = Retry.DefaultPolicy("strict-succession-plan-override");
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
}
