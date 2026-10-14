namespace Acme.Workforce.Compensation.Adjustment.Allocation;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of adjustment allocation policy.</summary>
public static class AdjustmentAllocationPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 36982.0, in order.</summary>
    public static int AdjustmentAllocationBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 36982.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string AdjustmentAllocationBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }
}
