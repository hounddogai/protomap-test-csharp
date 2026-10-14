namespace Acme.Workforce.Assets.Warranty.Assignment;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of warranty profile policy.</summary>
public static class WarrantyProfilePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 64729.0, in order.</summary>
    public static int WarrantyProfileBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 64729.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string WarrantyProfileBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}
