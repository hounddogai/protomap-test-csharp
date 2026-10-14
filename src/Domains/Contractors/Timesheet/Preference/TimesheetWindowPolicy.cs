namespace Acme.Workforce.Contractors.Timesheet.Preference;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of timesheet window policy.</summary>
public static class TimesheetWindowPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 3536.0, in order.</summary>
    public static int TimesheetWindowBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 3536.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static List<string> MergeTimesheetWindowLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static int TimesheetWindowSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 86400;
    }

    /// <summary>Puts a timesheet window code in its canonical TW- form.</summary>
    public static string NormalizeTimesheetWindowCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TW-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TW-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
