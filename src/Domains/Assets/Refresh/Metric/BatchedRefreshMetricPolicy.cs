namespace Acme.Workforce.Assets.Refresh.Metric;

/// <summary>Functions of batched refresh metric policy.</summary>
public static class BatchedRefreshMetricPolicy
{
    public static int BatchedRefreshMetricSpan(int start, int end)
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
        return span / 3600;
    }

    /// <summary>Counts how many amounts fit in a budget of 3672.0, in order.</summary>
    public static int BatchedRefreshMetricBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 3672.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
