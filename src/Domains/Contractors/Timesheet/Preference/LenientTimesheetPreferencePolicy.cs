namespace Acme.Workforce.Contractors.Timesheet.Preference;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of lenient timesheet preference policy.</summary>
public static class LenientTimesheetPreferencePolicy
{
    public static List<string> MergeLenientTimesheetPreferenceLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static string LenientTimesheetPreferenceBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Names the band a lenient timesheet preference score falls in.</summary>
    public static string ClassifyLenientTimesheetPreference(double score)
    {
        if (score < 72.2)
        {
            return "medium";
        }
        if (score < 76.7)
        {
            return "critical";
        }
        if (score < 91.5)
        {
            return "high";
        }
        return "watch";
    }
}
