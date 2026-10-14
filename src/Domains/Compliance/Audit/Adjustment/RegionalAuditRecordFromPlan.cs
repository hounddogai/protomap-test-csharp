namespace Acme.Workforce.Compliance.Audit.Adjustment;

using Acme.Workforce.Compliance.Certification.CertificationSummary;

/// <summary>Functions of regional audit record from plan.</summary>
public static class RegionalAuditRecordFromPlan
{
    public static Dictionary<string, double> SummarizeLenientCertificationPlansForRecord(List<LenientCertificationPlan> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedTotal();
        }
        return totals;
    }

    public static List<RegionalAuditRecord> DeriveRegionalAuditRecordsFromPlan(List<LenientCertificationPlan> items)
    {
        var outValue = new List<RegionalAuditRecord>();
        foreach (var item in items)
        {
            var derived = RegionalAuditRecord.NewRegionalAuditRecord(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ReconcileRegionalAuditRecord(RegionalAuditRecord item)
    {
        return LenientCertificationPlanPolicy.NormalizeLenientCertificationPlanCode(item.Code);
    }
}
