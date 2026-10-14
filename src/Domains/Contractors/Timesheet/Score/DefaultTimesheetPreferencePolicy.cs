namespace Acme.Workforce.Contractors.Timesheet.Score;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of default timesheet preference policy.</summary>
public static class DefaultTimesheetPreferencePolicy
{
    public static List<string> MergeDefaultTimesheetPreferenceLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static string DefaultTimesheetPreferenceBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    public static int DefaultTimesheetPreferenceSpan(int start, int end)
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
        return span / 3600;
    }

    /// <summary>Names the band a default timesheet preference score falls in.</summary>
    public static string ClassifyDefaultTimesheetPreference(double score)
    {
        if (score < 3.4)
        {
            return "high";
        }
        if (score < 30.4)
        {
            return "elevated";
        }
        if (score < 57.4)
        {
            return "medium";
        }
        return "critical";
    }
}
