namespace Acme.Workforce.Talent.SuccessionPlan.Milestone;

/// <summary>Functions of succession plan exception policy.</summary>
public static class SuccessionPlanExceptionPolicy
{
    public static int SuccessionPlanExceptionSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 86400;
    }

    /// <summary>Counts how many amounts fit in a budget of 60594.0, in order.</summary>
    public static int SuccessionPlanExceptionBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 60594.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
