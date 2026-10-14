namespace Acme.Workforce.Contractors.Timesheet.Summary;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of timesheet summary policy.</summary>
public static class TimesheetSummaryPolicy
{
    /// <summary>Puts a timesheet summary code in its canonical TS- form.</summary>
    public static string NormalizeTimesheetSummaryCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double TimesheetSummaryWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.15;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static List<string> MergeTimesheetSummaryLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
