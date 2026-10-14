namespace Acme.Workforce.Contractors.Engagement.EngagementAssignment;

/// <summary>Functions of engagement exception policy.</summary>
public static class EngagementExceptionPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 65389.0, in order.</summary>
    public static int EngagementExceptionBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 65389.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int EngagementExceptionSpan(int start, int end)
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
        return span / 60;
    }
}
