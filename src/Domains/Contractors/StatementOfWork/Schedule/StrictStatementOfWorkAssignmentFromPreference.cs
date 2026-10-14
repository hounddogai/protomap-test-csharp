namespace Acme.Workforce.Contractors.StatementOfWork.Schedule;

using Acme.Workforce.Contractors.Timesheet.Score;

/// <summary>Functions of strict statement of work assignment from preference.</summary>
public static class StrictStatementOfWorkAssignmentFromPreference
{
    public static Dictionary<string, double> SummarizeDefaultTimesheetPreferencesForAssignment(List<DefaultTimesheetPreference> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.MeasuredValue();
        }
        return totals;
    }

    public static List<StrictStatementOfWorkAssignment> DeriveStrictStatementOfWorkAssignmentsFromPreference(List<DefaultTimesheetPreference> items)
    {
        var outValue = new List<StrictStatementOfWorkAssignment>();
        foreach (var item in items)
        {
            var derived = StrictStatementOfWorkAssignment.NewStrictStatementOfWorkAssignment(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string RankStrictStatementOfWorkAssignment(StrictStatementOfWorkAssignment item)
    {
        return ScheduledTimesheetScorePolicy.ClassifyScheduledTimesheetScore(item.AdjustedLoad());
    }
}
