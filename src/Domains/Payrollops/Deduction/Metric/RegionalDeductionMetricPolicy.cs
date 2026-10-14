namespace Acme.Workforce.Payrollops.Deduction.Metric;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of regional deduction metric policy.</summary>
public static class RegionalDeductionMetricPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 23423.0, in order.</summary>
    public static int RegionalDeductionMetricBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 23423.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double RegionalDeductionMetricWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.12;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string RegionalDeductionMetricBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }
}
