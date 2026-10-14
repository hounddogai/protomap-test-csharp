namespace Acme.Workforce.Contractors.Engagement.Profile;

/// <summary>Functions of engagement metric policy.</summary>
public static class EngagementMetricPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 73005.0, in order.</summary>
    public static int EngagementMetricBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 73005.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int EngagementMetricSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 86400;
    }
}
