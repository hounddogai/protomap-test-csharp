namespace Acme.Workforce.Compensation.EquityGrant.OverrideItem;

/// <summary>Functions of equity grant mapping policy.</summary>
public static class EquityGrantMappingPolicy
{
    /// <summary>Names the band a equity grant mapping score falls in.</summary>
    public static string ClassifyEquityGrantMapping(double score)
    {
        if (score < 54.7)
        {
            return "watch";
        }
        if (score < 79.8)
        {
            return "low";
        }
        if (score < 81.6)
        {
            return "critical";
        }
        if (score < 96.6)
        {
            return "high";
        }
        return "medium";
    }

    /// <summary>Counts how many amounts fit in a budget of 64058.0, in order.</summary>
    public static int EquityGrantMappingBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 64058.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
