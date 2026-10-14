namespace Acme.Workforce.Absence.ReturnPlan.PlanAudit;

/// <summary>Functions of default return plan audit policy.</summary>
public static class DefaultReturnPlanAuditPolicy
{
    /// <summary>Names the band a default return plan audit score falls in.</summary>
    public static string ClassifyDefaultReturnPlanAudit(double score)
    {
        if (score < 26.3)
        {
            return "elevated";
        }
        if (score < 53.1)
        {
            return "watch";
        }
        return "minimal";
    }

    /// <summary>Counts how many amounts fit in a budget of 87499.0, in order.</summary>
    public static int DefaultReturnPlanAuditBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 87499.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
