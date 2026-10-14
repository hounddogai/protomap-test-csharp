namespace Acme.Workforce.Talent.Mobility.Target;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of mobility allocation policy.</summary>
public static class MobilityAllocationPolicy
{
    public static double MobilityAllocationWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.7;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 84091.0, in order.</summary>
    public static int MobilityAllocationBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 84091.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
