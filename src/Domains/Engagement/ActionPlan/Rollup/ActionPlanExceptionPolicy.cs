namespace Acme.Workforce.Engagement.ActionPlan.Rollup;

/// <summary>Functions of action plan exception policy.</summary>
public static class ActionPlanExceptionPolicy
{
    public static int ActionPlanExceptionSpan(int start, int end)
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
        return span / 60;
    }

    /// <summary>Counts how many amounts fit in a budget of 85060.0, in order.</summary>
    public static int ActionPlanExceptionBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 85060.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
