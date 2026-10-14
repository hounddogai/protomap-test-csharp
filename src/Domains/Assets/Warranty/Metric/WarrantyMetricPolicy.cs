namespace Acme.Workforce.Assets.Warranty.Metric;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of warranty metric policy.</summary>
public static class WarrantyMetricPolicy
{
    public static double WarrantyMetricWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.97;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 22836.0, in order.</summary>
    public static int WarrantyMetricBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 22836.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
