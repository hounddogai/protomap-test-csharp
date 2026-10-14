namespace Acme.Workforce.Compliance.Audit.Adjustment;

using Acme.Workforce.Compliance.Audit.RecordItem;

/// <summary>Functions of primary audit schedule from record.</summary>
public static class PrimaryAuditScheduleFromRecord
{
    public static Dictionary<string, double> SummarizePendingAuditRecordsForSchedule(List<PendingAuditRecord> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetValue();
        }
        return totals;
    }

    public static List<PrimaryAuditSchedule> DerivePrimaryAuditSchedulesFromRecord(List<PendingAuditRecord> items)
    {
        var outValue = new List<PrimaryAuditSchedule>();
        foreach (var item in items)
        {
            var derived = PrimaryAuditSchedule.NewPrimaryAuditSchedule(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string PlanPrimaryAuditSchedule(PrimaryAuditSchedule item)
    {
        return ManualAuditSchedulePolicy.ManualAuditScheduleBucket(item.AdjustedExposure());
    }
}
