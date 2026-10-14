namespace Acme.Workforce.Absence.Balance.Tier;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of balance tier policy.</summary>
public static class BalanceTierPolicy
{
    public static double BalanceTierWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.86;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 44930.0, in order.</summary>
    public static int BalanceTierBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 44930.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
