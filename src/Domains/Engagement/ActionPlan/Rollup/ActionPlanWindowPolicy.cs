namespace Acme.Workforce.Engagement.ActionPlan.Rollup;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of action plan window policy.</summary>
public static class ActionPlanWindowPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 67732.0, in order.</summary>
    public static int ActionPlanWindowBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 67732.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a action plan window score falls in.</summary>
    public static string ClassifyActionPlanWindow(double score)
    {
        if (score < 22.3)
        {
            return "elevated";
        }
        if (score < 23.7)
        {
            return "low";
        }
        return "high";
    }

    public static int ActionPlanWindowSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 86400;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeActionPlanWindow()
    {
        var policy = Retry.DefaultPolicy("action-plan-window");
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
}
