namespace Acme.Workforce.Absence.Balance.Assignment;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of balance assignment policy.</summary>
public static class BalanceAssignmentPolicy
{
    public static double BalanceAssignmentWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.12;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 31695.0, in order.</summary>
    public static int BalanceAssignmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 31695.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
