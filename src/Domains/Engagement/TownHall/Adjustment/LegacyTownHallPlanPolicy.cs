namespace Acme.Workforce.Engagement.TownHall.Adjustment;

/// <summary>Functions of legacy town hall plan policy.</summary>
public static class LegacyTownHallPlanPolicy
{
    /// <summary>Names the band a legacy town hall plan score falls in.</summary>
    public static string ClassifyLegacyTownHallPlan(double score)
    {
        if (score < 6.2)
        {
            return "ok";
        }
        if (score < 84.1)
        {
            return "high";
        }
        if (score < 96.8)
        {
            return "elevated";
        }
        if (score < 98.8)
        {
            return "watch";
        }
        return "minimal";
    }

    /// <summary>Counts how many amounts fit in a budget of 47933.0, in order.</summary>
    public static int LegacyTownHallPlanBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 47933.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
