namespace Acme.Workforce.Compliance.Attestation.AttestationDraft;

using Acme.Workforce.Compliance.Audit.AuditBaseline;

/// <summary>Functions of pending attestation threshold from template.</summary>
public static class PendingAttestationThresholdFromTemplate
{
    public static Dictionary<string, double> SummarizeBatchedAuditTemplatesForThreshold(List<BatchedAuditTemplate> items)
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

    public static List<PendingAttestationThreshold> DerivePendingAttestationThresholdsFromTemplate(List<BatchedAuditTemplate> items)
    {
        var outValue = new List<PendingAttestationThreshold>();
        foreach (var item in items)
        {
            var derived = PendingAttestationThreshold.NewPendingAttestationThreshold(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string PlanPendingAttestationThreshold(PendingAttestationThreshold item)
    {
        return GlobalAuditMappingPolicy.ClassifyGlobalAuditMapping(item.MeasuredValue());
    }
}
