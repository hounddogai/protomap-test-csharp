namespace Acme.Workforce.Absence.Balance.RecordItem;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of fallback balance threshold policy.</summary>
public static class FallbackBalanceThresholdPolicy
{
    public static int FallbackBalanceThresholdSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 86400;
    }

    /// <summary>Counts how many amounts fit in a budget of 50852.0, in order.</summary>
    public static int FallbackBalanceThresholdBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 50852.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double FallbackBalanceThresholdWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 2.43;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
