namespace Acme.Workforce.Compensation.MeritCycle.CycleSegment;

/// <summary>Functions of adaptive merit cycle threshold policy.</summary>
public static class AdaptiveMeritCycleThresholdPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 53615.0, in order.</summary>
    public static int AdaptiveMeritCycleThresholdBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 53615.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int AdaptiveMeritCycleThresholdSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }
}
