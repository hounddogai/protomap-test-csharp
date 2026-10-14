namespace Acme.Workforce.Contractors.Timesheet.Rollup;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of capped timesheet plan policy.</summary>
public static class CappedTimesheetPlanPolicy
{
    public static List<string> MergeCappedTimesheetPlanLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static int CappedTimesheetPlanSpan(int start, int end)
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
}
