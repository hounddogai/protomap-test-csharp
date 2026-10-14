namespace Acme.Workforce.Contractors.Timesheet.Request;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of scheduled timesheet target policy.</summary>
public static class ScheduledTimesheetTargetPolicy
{
    /// <summary>Names the band a scheduled timesheet target score falls in.</summary>
    public static string ClassifyScheduledTimesheetTarget(double score)
    {
        if (score < 10.5)
        {
            return "critical";
        }
        if (score < 74.8)
        {
            return "high";
        }
        return "ok";
    }

    public static List<string> MergeScheduledTimesheetTargetLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static double ScheduledTimesheetTargetWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.11;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string ScheduledTimesheetTargetBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}
