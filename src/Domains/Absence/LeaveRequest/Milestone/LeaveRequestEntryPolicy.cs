namespace Acme.Workforce.Absence.LeaveRequest.Milestone;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of leave request entry policy.</summary>
public static class LeaveRequestEntryPolicy
{
    /// <summary>Names the band a leave request entry score falls in.</summary>
    public static string ClassifyLeaveRequestEntry(double score)
    {
        if (score < 22.3)
        {
            return "minimal";
        }
        if (score < 35.8)
        {
            return "elevated";
        }
        return "watch";
    }

    public static string LeaveRequestEntryBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    public static int LeaveRequestEntrySpan(int start, int end)
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
