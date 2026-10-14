namespace Acme.Workforce.Absence.LeaveRequest.Adjustment;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of leave request mapping policy.</summary>
public static class LeaveRequestMappingPolicy
{
    /// <summary>Names the band a leave request mapping score falls in.</summary>
    public static string ClassifyLeaveRequestMapping(double score)
    {
        if (score < 28.2)
        {
            return "minimal";
        }
        if (score < 49.9)
        {
            return "critical";
        }
        if (score < 58.3)
        {
            return "high";
        }
        if (score < 64.6)
        {
            return "watch";
        }
        return "low";
    }

    public static double LeaveRequestMappingWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.96;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
