namespace Acme.Workforce.Compliance.Audit.AuditBaseline;

using Acme.Workforce.Compliance.Attestation.Adjustment;

/// <summary>Functions of adaptive audit baseline from window.</summary>
public static class AdaptiveAuditBaselineFromWindow
{
    public static Dictionary<string, double> SummarizeBatchedAttestationWindowsForBaseline(List<BatchedAttestationWindow> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedLoad();
        }
        return totals;
    }

    public static List<AdaptiveAuditBaseline> DeriveAdaptiveAuditBaselinesFromWindow(List<BatchedAttestationWindow> items)
    {
        var outValue = new List<AdaptiveAuditBaseline>();
        foreach (var item in items)
        {
            var derived = AdaptiveAuditBaseline.NewAdaptiveAuditBaseline(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputeAdaptiveAuditBaseline(AdaptiveAuditBaseline item)
    {
        return BatchedAttestationWindowPolicy.NormalizeBatchedAttestationWindowCode(item.Code);
    }
}
