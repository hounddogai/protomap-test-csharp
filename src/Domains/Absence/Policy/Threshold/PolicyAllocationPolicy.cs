namespace Acme.Workforce.Absence.Policy.Threshold;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of policy allocation policy.</summary>
public static class PolicyAllocationPolicy
{
    public static double PolicyAllocationWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 2.48;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 5606.0, in order.</summary>
    public static int PolicyAllocationBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 5606.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
