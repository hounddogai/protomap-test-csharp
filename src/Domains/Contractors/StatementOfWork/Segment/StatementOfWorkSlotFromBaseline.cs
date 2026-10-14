namespace Acme.Workforce.Contractors.StatementOfWork.Segment;

using Acme.Workforce.Contractors.Timesheet.Summary;

/// <summary>Functions of statement of work slot from baseline.</summary>
public static class StatementOfWorkSlotFromBaseline
{
    public static Dictionary<string, double> SummarizeTimesheetBaselinesForSlot(List<TimesheetBaseline> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetTotal();
        }
        return totals;
    }

    public static List<StatementOfWorkSlot> DeriveStatementOfWorkSlotsFromBaseline(List<TimesheetBaseline> items)
    {
        var outValue = new List<StatementOfWorkSlot>();
        foreach (var item in items)
        {
            var derived = StatementOfWorkSlot.NewStatementOfWorkSlot(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchStatementOfWorkSlot(StatementOfWorkSlot item)
    {
        return TimesheetBaselinePolicy.TimesheetBaselineBucket(item.ProjectedLoad());
    }
}
