namespace Acme.Workforce.Engagement.ActionPlan.RecordItem;

/// <summary>Functions of pending action plan rollup policy.</summary>
public static class PendingActionPlanRollupPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 96236.0, in order.</summary>
    public static int PendingActionPlanRollupBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 96236.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a pending action plan rollup score falls in.</summary>
    public static string ClassifyPendingActionPlanRollup(double score)
    {
        if (score < 7.6)
        {
            return "minimal";
        }
        if (score < 21.2)
        {
            return "low";
        }
        if (score < 72.5)
        {
            return "medium";
        }
        if (score < 91.8)
        {
            return "elevated";
        }
        return "critical";
    }
}
