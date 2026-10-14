namespace Acme.Workforce.Contractors.Timesheet.TimesheetProfile;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of timesheet group policy.</summary>
public static class TimesheetGroupPolicy
{
    public static double TimesheetGroupWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 0.89;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static List<string> MergeTimesheetGroupLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a timesheet group score falls in.</summary>
    public static string ClassifyTimesheetGroup(double score)
    {
        if (score < 24.0)
        {
            return "low";
        }
        if (score < 40.7)
        {
            return "high";
        }
        if (score < 45.8)
        {
            return "elevated";
        }
        return "watch";
    }

    /// <summary>Puts a timesheet group code in its canonical TG- form.</summary>
    public static string NormalizeTimesheetGroupCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TG-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TG-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
