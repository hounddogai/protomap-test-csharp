namespace Acme.Workforce.Absence.LeaveRequest.Plan;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of scheduled leave request exception policy.</summary>
public static class ScheduledLeaveRequestExceptionPolicy
{
    public static int ScheduledLeaveRequestExceptionSpan(int start, int end)
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

    public static List<string> MergeScheduledLeaveRequestExceptionLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a scheduled leave request exception score falls in.</summary>
    public static string ClassifyScheduledLeaveRequestException(double score)
    {
        if (score < 31.6)
        {
            return "high";
        }
        if (score < 63.0)
        {
            return "ok";
        }
        return "critical";
    }

    public static double ScheduledLeaveRequestExceptionWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 0.46;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
