namespace Acme.Workforce.Assets.Warranty.Rollup;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of primary warranty rollup policy.</summary>
public static class PrimaryWarrantyRollupPolicy
{
    /// <summary>Names the band a primary warranty rollup score falls in.</summary>
    public static string ClassifyPrimaryWarrantyRollup(double score)
    {
        if (score < 34.3)
        {
            return "ok";
        }
        if (score < 75.5)
        {
            return "watch";
        }
        return "medium";
    }

    /// <summary>Counts how many amounts fit in a budget of 36359.0, in order.</summary>
    public static int PrimaryWarrantyRollupBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 36359.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double PrimaryWarrantyRollupWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.82;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
