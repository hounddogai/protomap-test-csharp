namespace Acme.Workforce.Workplace.Visitor.Adjustment;

/// <summary>Functions of capped visitor assignment policy.</summary>
public static class CappedVisitorAssignmentPolicy
{
    /// <summary>Names the band a capped visitor assignment score falls in.</summary>
    public static string ClassifyCappedVisitorAssignment(double score)
    {
        if (score < 54.5)
        {
            return "minimal";
        }
        if (score < 83.0)
        {
            return "elevated";
        }
        return "watch";
    }

    public static int CappedVisitorAssignmentSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }

    /// <summary>Counts how many amounts fit in a budget of 73978.0, in order.</summary>
    public static int CappedVisitorAssignmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 73978.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
