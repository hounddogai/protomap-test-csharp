namespace Acme.Workforce.Absence.ReturnPlan.PlanAudit;

/// <summary>Functions of strict return plan rollup policy.</summary>
public static class StrictReturnPlanRollupPolicy
{
    /// <summary>Names the band a strict return plan rollup score falls in.</summary>
    public static string ClassifyStrictReturnPlanRollup(double score)
    {
        if (score < 23.8)
        {
            return "ok";
        }
        if (score < 36.8)
        {
            return "low";
        }
        if (score < 49.3)
        {
            return "elevated";
        }
        if (score < 64.7)
        {
            return "critical";
        }
        return "high";
    }

    /// <summary>Counts how many amounts fit in a budget of 65857.0, in order.</summary>
    public static int StrictReturnPlanRollupBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 65857.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
