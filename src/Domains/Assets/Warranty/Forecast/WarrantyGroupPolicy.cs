namespace Acme.Workforce.Assets.Warranty.Forecast;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of warranty group policy.</summary>
public static class WarrantyGroupPolicy
{
    public static double WarrantyGroupWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.86;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 56216.0, in order.</summary>
    public static int WarrantyGroupBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 56216.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
