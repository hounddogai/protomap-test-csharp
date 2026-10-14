namespace Acme.Workforce.Compensation.Adjustment.Schedule;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of batched adjustment schedule policy.</summary>
public static class BatchedAdjustmentSchedulePolicy
{
    public static int BatchedAdjustmentScheduleSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 86400;
    }

    public static double BatchedAdjustmentScheduleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 0.2;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 30544.0, in order.</summary>
    public static int BatchedAdjustmentScheduleBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 30544.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string BatchedAdjustmentScheduleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }
}
