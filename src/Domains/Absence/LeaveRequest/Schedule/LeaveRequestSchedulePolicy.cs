namespace Acme.Workforce.Absence.LeaveRequest.Schedule;

/// <summary>Functions of leave request schedule policy.</summary>
public static class LeaveRequestSchedulePolicy
{
    public static int LeaveRequestScheduleSpan(int start, int end)
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

    /// <summary>Names the band a leave request schedule score falls in.</summary>
    public static string ClassifyLeaveRequestSchedule(double score)
    {
        if (score < 23.3)
        {
            return "watch";
        }
        if (score < 41.8)
        {
            return "medium";
        }
        return "elevated";
    }
}
