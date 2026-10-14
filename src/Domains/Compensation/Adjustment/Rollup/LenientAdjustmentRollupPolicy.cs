namespace Acme.Workforce.Compensation.Adjustment.Rollup;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of lenient adjustment rollup policy.</summary>
public static class LenientAdjustmentRollupPolicy
{
    /// <summary>Names the band a lenient adjustment rollup score falls in.</summary>
    public static string ClassifyLenientAdjustmentRollup(double score)
    {
        if (score < 15.3)
        {
            return "elevated";
        }
        if (score < 74.1)
        {
            return "medium";
        }
        return "high";
    }

    public static int LenientAdjustmentRollupSpan(int start, int end)
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
        return span / 3600;
    }

    /// <summary>Counts how many amounts fit in a budget of 60849.0, in order.</summary>
    public static int LenientAdjustmentRollupBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 60849.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double LenientAdjustmentRollupWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.65;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
