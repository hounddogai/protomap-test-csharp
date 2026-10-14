namespace Acme.Workforce.Compensation.Adjustment.Summary;

/// <summary>Functions of adjustment metric policy.</summary>
public static class AdjustmentMetricPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 88221.0, in order.</summary>
    public static int AdjustmentMetricBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 88221.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int AdjustmentMetricSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 86400;
    }
}
