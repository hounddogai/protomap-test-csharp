namespace Acme.Workforce.Contractors.Engagement.Summary;

/// <summary>Functions of capped engagement summary policy.</summary>
public static class CappedEngagementSummaryPolicy
{
    /// <summary>Names the band a capped engagement summary score falls in.</summary>
    public static string ClassifyCappedEngagementSummary(double score)
    {
        if (score < 31.3)
        {
            return "medium";
        }
        if (score < 95.1)
        {
            return "watch";
        }
        return "high";
    }

    public static int CappedEngagementSummarySpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 86400;
    }

    /// <summary>Counts how many amounts fit in a budget of 18214.0, in order.</summary>
    public static int CappedEngagementSummaryBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 18214.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
