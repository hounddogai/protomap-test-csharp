namespace Acme.Workforce.Compensation.Adjustment.Notice;

/// <summary>Functions of adjustment snapshot policy.</summary>
public static class AdjustmentSnapshotPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 11155.0, in order.</summary>
    public static int AdjustmentSnapshotBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 11155.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int AdjustmentSnapshotSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 86400;
    }
}
