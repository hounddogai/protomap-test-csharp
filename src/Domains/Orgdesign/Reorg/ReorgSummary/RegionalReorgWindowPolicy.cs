namespace Acme.Workforce.Orgdesign.Reorg.ReorgSummary;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of regional reorg window policy.</summary>
public static class RegionalReorgWindowPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 83895.0, in order.</summary>
    public static int RegionalReorgWindowBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 83895.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a regional reorg window score falls in.</summary>
    public static string ClassifyRegionalReorgWindow(double score)
    {
        if (score < 5.7)
        {
            return "low";
        }
        if (score < 84.7)
        {
            return "minimal";
        }
        return "critical";
    }

    public static double RegionalReorgWindowWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.11;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
