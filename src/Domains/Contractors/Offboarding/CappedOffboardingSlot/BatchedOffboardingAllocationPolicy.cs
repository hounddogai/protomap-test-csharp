namespace Acme.Workforce.Contractors.Offboarding.CappedOffboardingSlot;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of batched offboarding allocation policy.</summary>
public static class BatchedOffboardingAllocationPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 24551.0, in order.</summary>
    public static int BatchedOffboardingAllocationBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 24551.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double BatchedOffboardingAllocationWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.2;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
