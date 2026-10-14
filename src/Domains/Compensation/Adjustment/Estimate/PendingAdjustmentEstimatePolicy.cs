namespace Acme.Workforce.Compensation.Adjustment.Estimate;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of pending adjustment estimate policy.</summary>
public static class PendingAdjustmentEstimatePolicy
{
    public static string PendingAdjustmentEstimateBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    public static int PendingAdjustmentEstimateSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 86400;
    }

    /// <summary>Counts how many amounts fit in a budget of 75228.0, in order.</summary>
    public static int PendingAdjustmentEstimateBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 75228.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
