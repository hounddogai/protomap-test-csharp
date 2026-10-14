namespace Acme.Workforce.Compensation.Adjustment.Rule;

/// <summary>Functions of adjustment rule policy.</summary>
public static class AdjustmentRulePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 95085.0, in order.</summary>
    public static int AdjustmentRuleBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 95085.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a adjustment rule score falls in.</summary>
    public static string ClassifyAdjustmentRule(double score)
    {
        if (score < 3.9)
        {
            return "elevated";
        }
        if (score < 14.7)
        {
            return "ok";
        }
        if (score < 40.5)
        {
            return "high";
        }
        if (score < 72.6)
        {
            return "low";
        }
        return "critical";
    }

    public static int AdjustmentRuleSpan(int start, int end)
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
}
