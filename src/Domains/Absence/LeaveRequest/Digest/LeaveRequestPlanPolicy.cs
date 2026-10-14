namespace Acme.Workforce.Absence.LeaveRequest.Digest;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of leave request plan policy.</summary>
public static class LeaveRequestPlanPolicy
{
    public static string LeaveRequestPlanBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Names the band a leave request plan score falls in.</summary>
    public static string ClassifyLeaveRequestPlan(double score)
    {
        if (score < 2.1)
        {
            return "watch";
        }
        if (score < 22.9)
        {
            return "low";
        }
        if (score < 27.7)
        {
            return "minimal";
        }
        if (score < 81.6)
        {
            return "critical";
        }
        return "ok";
    }
}
