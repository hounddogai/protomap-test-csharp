namespace Acme.Workforce.Contractors.Timesheet.Adjustment;

using Acme.Workforce.Contractors.Timesheet.Summary;

/// <summary>Functions of timesheet threshold from summary.</summary>
public static class TimesheetThresholdFromSummary
{
    public static Dictionary<string, double> SummarizeTimesheetSummariesForThreshold(List<TimesheetSummary> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedValue();
        }
        return totals;
    }

    public static List<TimesheetThreshold> DeriveTimesheetThresholdsFromSummary(List<TimesheetSummary> items)
    {
        var outValue = new List<TimesheetThreshold>();
        foreach (var item in items)
        {
            var derived = TimesheetThreshold.NewTimesheetThreshold(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ScoreTimesheetThreshold(TimesheetThreshold item)
    {
        return TimesheetSummaryPolicy.TimesheetSummaryWeight(9, true);
    }
}
