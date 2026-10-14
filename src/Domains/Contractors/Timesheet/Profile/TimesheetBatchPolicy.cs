namespace Acme.Workforce.Contractors.Timesheet.Profile;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of timesheet batch policy.</summary>
public static class TimesheetBatchPolicy
{
    /// <summary>Puts a timesheet batch code in its canonical TB- form.</summary>
    public static string NormalizeTimesheetBatchCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeTimesheetBatchLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
