namespace Acme.Workforce.Contractors.Timesheet.Summary;

using Acme.Workforce.Contractors.Timesheet.Reservation;

/// <summary>Functions of timesheet summary from score.</summary>
public static class TimesheetSummaryFromScore
{
    public static Dictionary<string, double> SummarizeTimesheetScoresForSummary(List<TimesheetScore> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetExposure();
        }
        return totals;
    }

    public static List<TimesheetSummary> DeriveTimesheetSummariesFromScore(List<TimesheetScore> items)
    {
        var outValue = new List<TimesheetSummary>();
        foreach (var item in items)
        {
            var derived = TimesheetSummary.NewTimesheetSummary(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScoreTimesheetSummary(TimesheetSummary item)
    {
        return TimesheetScorePolicy.ClassifyTimesheetScore(item.WeightedValue());
    }
}
