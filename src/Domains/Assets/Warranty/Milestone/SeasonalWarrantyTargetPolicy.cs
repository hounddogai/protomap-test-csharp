namespace Acme.Workforce.Assets.Warranty.Milestone;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of seasonal warranty target policy.</summary>
public static class SeasonalWarrantyTargetPolicy
{
    public static string SeasonalWarrantyTargetBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Names the band a seasonal warranty target score falls in.</summary>
    public static string ClassifySeasonalWarrantyTarget(double score)
    {
        if (score < 22.7)
        {
            return "critical";
        }
        if (score < 37.1)
        {
            return "low";
        }
        if (score < 61.1)
        {
            return "ok";
        }
        if (score < 97.8)
        {
            return "minimal";
        }
        return "elevated";
    }

    /// <summary>Counts how many amounts fit in a budget of 9762.0, in order.</summary>
    public static int SeasonalWarrantyTargetBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 9762.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
