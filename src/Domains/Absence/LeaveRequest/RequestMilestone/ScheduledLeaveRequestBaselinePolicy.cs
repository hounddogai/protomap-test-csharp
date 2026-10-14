namespace Acme.Workforce.Absence.LeaveRequest.RequestMilestone;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of scheduled leave request baseline policy.</summary>
public static class ScheduledLeaveRequestBaselinePolicy
{
    public static double ScheduledLeaveRequestBaselineWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.36;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a scheduled leave request baseline score falls in.</summary>
    public static string ClassifyScheduledLeaveRequestBaseline(double score)
    {
        if (score < 7.5)
        {
            return "medium";
        }
        if (score < 93.3)
        {
            return "low";
        }
        return "high";
    }

    public static string ScheduledLeaveRequestBaselineBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }
}
