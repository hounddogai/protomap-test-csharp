namespace Acme.Workforce.Contractors.Timesheet.Exception;

using Acme.Workforce.Contractors.StatementOfWork.Baseline;

/// <summary>Functions of timesheet assignment from variance.</summary>
public static class TimesheetAssignmentFromVariance
{
    public static Dictionary<string, double> SummarizeScheduledStatementOfWorkVariancesForAssignment(List<ScheduledStatementOfWorkVariance> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedTotal();
        }
        return totals;
    }

    public static List<TimesheetAssignment> DeriveTimesheetAssignmentsFromVariance(List<ScheduledStatementOfWorkVariance> items)
    {
        var outValue = new List<TimesheetAssignment>();
        foreach (var item in items)
        {
            var derived = TimesheetAssignment.NewTimesheetAssignment(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveTimesheetAssignment(TimesheetAssignment item)
    {
        return PrimaryStatementOfWorkAllocationPolicy.ClassifyPrimaryStatementOfWorkAllocation(item.NetVolume());
    }
}
