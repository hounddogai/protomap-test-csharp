namespace Acme.Workforce.Workplace.AccessBadge.Threshold;

/// <summary>Functions of regional access badge threshold policy.</summary>
public static class RegionalAccessBadgeThresholdPolicy
{
    /// <summary>Names the band a regional access badge threshold score falls in.</summary>
    public static string ClassifyRegionalAccessBadgeThreshold(double score)
    {
        if (score < 29.1)
        {
            return "critical";
        }
        if (score < 36.9)
        {
            return "minimal";
        }
        if (score < 42.4)
        {
            return "medium";
        }
        if (score < 57.5)
        {
            return "ok";
        }
        return "watch";
    }

    /// <summary>Counts how many amounts fit in a budget of 51842.0, in order.</summary>
    public static int RegionalAccessBadgeThresholdBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 51842.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
