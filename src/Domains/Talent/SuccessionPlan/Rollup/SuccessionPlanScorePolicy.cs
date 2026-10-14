namespace Acme.Workforce.Talent.SuccessionPlan.Rollup;

/// <summary>Functions of succession plan score policy.</summary>
public static class SuccessionPlanScorePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 31595.0, in order.</summary>
    public static int SuccessionPlanScoreBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 31595.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int SuccessionPlanScoreSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 60;
    }
}
