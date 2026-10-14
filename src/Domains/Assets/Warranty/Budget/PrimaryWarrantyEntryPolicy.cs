namespace Acme.Workforce.Assets.Warranty.Budget;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of primary warranty entry policy.</summary>
public static class PrimaryWarrantyEntryPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 37412.0, in order.</summary>
    public static int PrimaryWarrantyEntryBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 37412.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double PrimaryWarrantyEntryWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.97;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string PrimaryWarrantyEntryBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    public static int PrimaryWarrantyEntrySpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }
}
