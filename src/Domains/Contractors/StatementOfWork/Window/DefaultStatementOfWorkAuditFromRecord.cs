namespace Acme.Workforce.Contractors.StatementOfWork.Window;

using Acme.Workforce.Contractors.Timesheet.RecordItem;

/// <summary>Functions of default statement of work audit from record.</summary>
public static class DefaultStatementOfWorkAuditFromRecord
{
    public static Dictionary<string, double> SummarizeTimesheetRecordsForAudit(List<TimesheetRecord> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedExposure();
        }
        return totals;
    }

    public static List<DefaultStatementOfWorkAudit> DeriveDefaultStatementOfWorkAuditsFromRecord(List<TimesheetRecord> items)
    {
        var outValue = new List<DefaultStatementOfWorkAudit>();
        foreach (var item in items)
        {
            var derived = DefaultStatementOfWorkAudit.NewDefaultStatementOfWorkAudit(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string PlanDefaultStatementOfWorkAudit(DefaultStatementOfWorkAudit item)
    {
        return TimesheetRecordPolicy.TimesheetRecordBucket(item.AdjustedExposure());
    }
}
