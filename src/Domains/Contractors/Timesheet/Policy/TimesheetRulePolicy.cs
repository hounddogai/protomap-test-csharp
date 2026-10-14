namespace Acme.Workforce.Contractors.Timesheet.Policy;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of timesheet rule policy.</summary>
public static class TimesheetRulePolicy
{
    public static List<string> MergeTimesheetRuleLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static string TimesheetRuleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    public static int TimesheetRuleSpan(int start, int end)
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
        return span / 60;
    }

    /// <summary>Puts a timesheet rule code in its canonical TR- form.</summary>
    public static string NormalizeTimesheetRuleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
