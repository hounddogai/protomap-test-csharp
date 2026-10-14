namespace Acme.Workforce.Contractors.Timesheet.Profile;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of timesheet preference policy.</summary>
public static class TimesheetPreferencePolicy
{
    /// <summary>Puts a timesheet preference code in its canonical TP- form.</summary>
    public static string NormalizeTimesheetPreferenceCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string TimesheetPreferenceBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    public static List<string> MergeTimesheetPreferenceLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
