namespace Acme.Workforce.Compliance.Certification.CertificationSummary;

using Acme.Workforce.Compliance.Audit.RecordItem;

/// <summary>Functions of lenient certification plan from record.</summary>
public static class LenientCertificationPlanFromRecord
{
    public static Dictionary<string, double> SummarizePendingAuditRecordsForPlan(List<PendingAuditRecord> items)
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

    public static List<LenientCertificationPlan> DeriveLenientCertificationPlansFromRecord(List<PendingAuditRecord> items)
    {
        var outValue = new List<LenientCertificationPlan>();
        foreach (var item in items)
        {
            var derived = LenientCertificationPlan.NewLenientCertificationPlan(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectLenientCertificationPlan(LenientCertificationPlan item)
    {
        return ManualAuditSchedulePolicy.ManualAuditScheduleBucket(item.WeightedTotal());
    }
}
