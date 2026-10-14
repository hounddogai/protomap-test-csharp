namespace Acme.Workforce.Assets.Warranty.Agreement;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of warranty rollup policy.</summary>
public static class WarrantyRollupPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 36704.0, in order.</summary>
    public static int WarrantyRollupBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 36704.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a warranty rollup score falls in.</summary>
    public static string ClassifyWarrantyRollup(double score)
    {
        if (score < 21.8)
        {
            return "elevated";
        }
        if (score < 25.3)
        {
            return "critical";
        }
        if (score < 78.4)
        {
            return "medium";
        }
        if (score < 92.1)
        {
            return "watch";
        }
        return "high";
    }

    public static double WarrantyRollupWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.65;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
