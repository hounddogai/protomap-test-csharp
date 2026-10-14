namespace Acme.Workforce.Absence.Policy.Cycle;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of policy adjustment policy.</summary>
public static class PolicyAdjustmentPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 74951.0, in order.</summary>
    public static int PolicyAdjustmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 74951.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double PolicyAdjustmentWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.71;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
