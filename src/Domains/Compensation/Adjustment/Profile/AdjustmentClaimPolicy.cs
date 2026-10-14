namespace Acme.Workforce.Compensation.Adjustment.Profile;

/// <summary>Functions of adjustment claim policy.</summary>
public static class AdjustmentClaimPolicy
{
    /// <summary>Names the band a adjustment claim score falls in.</summary>
    public static string ClassifyAdjustmentClaim(double score)
    {
        if (score < 20.9)
        {
            return "medium";
        }
        if (score < 27.5)
        {
            return "watch";
        }
        if (score < 78.0)
        {
            return "ok";
        }
        if (score < 90.9)
        {
            return "high";
        }
        return "critical";
    }

    /// <summary>Counts how many amounts fit in a budget of 63552.0, in order.</summary>
    public static int AdjustmentClaimBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 63552.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
