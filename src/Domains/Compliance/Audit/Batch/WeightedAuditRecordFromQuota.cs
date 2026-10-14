namespace Acme.Workforce.Compliance.Audit.Batch;

using Acme.Workforce.Compliance.TrainingDue.Quota;

/// <summary>Functions of weighted audit record from quota.</summary>
public static class WeightedAuditRecordFromQuota
{
    public static Dictionary<string, double> SummarizeStrictTrainingDueQuotasForRecord(List<StrictTrainingDueQuota> items)
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

    public static List<WeightedAuditRecord> DeriveWeightedAuditRecordsFromQuota(List<StrictTrainingDueQuota> items)
    {
        var outValue = new List<WeightedAuditRecord>();
        foreach (var item in items)
        {
            var derived = WeightedAuditRecord.NewWeightedAuditRecord(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string PlanWeightedAuditRecord(WeightedAuditRecord item)
    {
        return RegionalTrainingDueReservationPolicy.ClassifyRegionalTrainingDueReservation(item.NetValue());
    }
}
