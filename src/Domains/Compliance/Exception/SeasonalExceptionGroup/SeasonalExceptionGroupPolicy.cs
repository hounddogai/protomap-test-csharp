namespace Acme.Workforce.Compliance.Exception.SeasonalExceptionGroup;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of seasonal exception group policy.</summary>
public static class SeasonalExceptionGroupPolicy
{
    /// <summary>Names the band a seasonal exception group score falls in.</summary>
    public static string ClassifySeasonalExceptionGroup(double score)
    {
        if (score < 36.6)
        {
            return "low";
        }
        if (score < 70.9)
        {
            return "minimal";
        }
        if (score < 79.7)
        {
            return "elevated";
        }
        return "medium";
    }

    public static double SeasonalExceptionGroupWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.27;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 63511.0, in order.</summary>
    public static int SeasonalExceptionGroupBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 63511.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
