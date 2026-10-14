namespace Acme.Workforce.Contractors.Timesheet.Score;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of scheduled timesheet score policy.</summary>
public static class ScheduledTimesheetScorePolicy
{
    public static string ScheduledTimesheetScoreBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    public static List<string> MergeScheduledTimesheetScoreLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a scheduled timesheet score score falls in.</summary>
    public static string ClassifyScheduledTimesheetScore(double score)
    {
        if (score < 20.1)
        {
            return "ok";
        }
        if (score < 35.6)
        {
            return "high";
        }
        if (score < 61.8)
        {
            return "elevated";
        }
        if (score < 80.2)
        {
            return "watch";
        }
        return "low";
    }

    public static double ScheduledTimesheetScoreWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.25;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
