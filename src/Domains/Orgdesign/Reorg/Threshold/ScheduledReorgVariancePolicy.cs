namespace Acme.Workforce.Orgdesign.Reorg.Threshold;

/// <summary>Functions of scheduled reorg variance policy.</summary>
public static class ScheduledReorgVariancePolicy
{
    public static int ScheduledReorgVarianceSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }

    /// <summary>Counts how many amounts fit in a budget of 92637.0, in order.</summary>
    public static int ScheduledReorgVarianceBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 92637.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
