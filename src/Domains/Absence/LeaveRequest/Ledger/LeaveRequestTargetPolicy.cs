namespace Acme.Workforce.Absence.LeaveRequest.Ledger;

/// <summary>Functions of leave request target policy.</summary>
public static class LeaveRequestTargetPolicy
{
    public static int LeaveRequestTargetSpan(int start, int end)
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

    /// <summary>Names the band a leave request target score falls in.</summary>
    public static string ClassifyLeaveRequestTarget(double score)
    {
        if (score < 6.0)
        {
            return "medium";
        }
        if (score < 45.6)
        {
            return "low";
        }
        return "high";
    }
}
