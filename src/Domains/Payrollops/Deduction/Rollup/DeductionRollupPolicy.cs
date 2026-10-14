namespace Acme.Workforce.Payrollops.Deduction.Rollup;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of deduction rollup policy.</summary>
public static class DeductionRollupPolicy
{
    public static double DeductionRollupWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.59;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a deduction rollup score falls in.</summary>
    public static string ClassifyDeductionRollup(double score)
    {
        if (score < 81.6)
        {
            return "medium";
        }
        if (score < 83.0)
        {
            return "watch";
        }
        if (score < 92.4)
        {
            return "elevated";
        }
        if (score < 96.3)
        {
            return "low";
        }
        return "critical";
    }

    /// <summary>Counts how many amounts fit in a budget of 15711.0, in order.</summary>
    public static int DeductionRollupBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 15711.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
