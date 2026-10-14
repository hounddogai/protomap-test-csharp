namespace Acme.Workforce.Contractors.Timesheet.TimesheetPolicy;

using Acme.Workforce.Contractors.Timesheet.Request;

/// <summary>Functions of timesheet bundle from cycle.</summary>
public static class TimesheetBundleFromCycle
{
    public static Dictionary<string, double> SummarizeTimesheetCyclesForBundle(List<TimesheetCycle> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedLoad();
        }
        return totals;
    }

    public static List<TimesheetBundle> DeriveTimesheetBundlesFromCycle(List<TimesheetCycle> items)
    {
        var outValue = new List<TimesheetBundle>();
        foreach (var item in items)
        {
            var derived = TimesheetBundle.NewTimesheetBundle(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string NormalizeTimesheetBundle(TimesheetBundle item)
    {
        return ScheduledTimesheetTargetPolicy.ClassifyScheduledTimesheetTarget(item.WeightedTotal());
    }
}
