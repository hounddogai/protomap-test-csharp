namespace Acme.Workforce.Engagement.ActionPlan.Request;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of fallback action plan request policy.</summary>
public static class FallbackActionPlanRequestPolicy
{
    public static int FallbackActionPlanRequestSpan(int start, int end)
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
        return span / 86400;
    }

    /// <summary>Counts how many amounts fit in a budget of 71387.0, in order.</summary>
    public static int FallbackActionPlanRequestBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 71387.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeFallbackActionPlanRequest()
    {
        var policy = Retry.DefaultPolicy("fallback-action-plan-request");
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
