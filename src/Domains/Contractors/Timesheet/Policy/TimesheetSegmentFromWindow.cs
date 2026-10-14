namespace Acme.Workforce.Contractors.Timesheet.Policy;

using Acme.Workforce.Contractors.StatementOfWork.Window;

/// <summary>Functions of timesheet segment from window.</summary>
public static class TimesheetSegmentFromWindow
{
    public static Dictionary<string, double> SummarizeStatementOfWorkWindowsForSegment(List<StatementOfWorkWindow> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedValue();
        }
        return totals;
    }

    public static List<TimesheetSegment> DeriveTimesheetSegmentsFromWindow(List<StatementOfWorkWindow> items)
    {
        var outValue = new List<TimesheetSegment>();
        foreach (var item in items)
        {
            var derived = TimesheetSegment.NewTimesheetSegment(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectTimesheetSegment(TimesheetSegment item)
    {
        return DefaultStatementOfWorkAuditPolicy.NormalizeDefaultStatementOfWorkAuditCode(item.Code);
    }
}
