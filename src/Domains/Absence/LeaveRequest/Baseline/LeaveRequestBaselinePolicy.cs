namespace Acme.Workforce.Absence.LeaveRequest.Baseline;

/// <summary>Functions of leave request baseline policy.</summary>
public static class LeaveRequestBaselinePolicy
{
    public static int LeaveRequestBaselineSpan(int start, int end)
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

    /// <summary>Names the band a leave request baseline score falls in.</summary>
    public static string ClassifyLeaveRequestBaseline(double score)
    {
        if (score < 65.4)
        {
            return "watch";
        }
        if (score < 73.4)
        {
            return "ok";
        }
        if (score < 81.1)
        {
            return "critical";
        }
        return "high";
    }
}
