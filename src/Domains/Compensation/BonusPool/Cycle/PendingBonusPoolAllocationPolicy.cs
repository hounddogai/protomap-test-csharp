namespace Acme.Workforce.Compensation.BonusPool.Cycle;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of pending bonus pool allocation policy.</summary>
public static class PendingBonusPoolAllocationPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 71316.0, in order.</summary>
    public static int PendingBonusPoolAllocationBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 71316.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string PendingBonusPoolAllocationBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    public static double PendingBonusPoolAllocationWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.59;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
