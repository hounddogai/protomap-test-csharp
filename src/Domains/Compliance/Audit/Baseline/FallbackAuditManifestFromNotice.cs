namespace Acme.Workforce.Compliance.Audit.Baseline;

using Acme.Workforce.Compliance.PolicyAck.Notice;

/// <summary>Functions of fallback audit manifest from notice.</summary>
public static class FallbackAuditManifestFromNotice
{
    public static Dictionary<string, double> SummarizePolicyAckNoticesForManifest(List<PolicyAckNotice> items)
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

    public static List<FallbackAuditManifest> DeriveFallbackAuditManifestsFromNotice(List<PolicyAckNotice> items)
    {
        var outValue = new List<FallbackAuditManifest>();
        foreach (var item in items)
        {
            var derived = FallbackAuditManifest.NewFallbackAuditManifest(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ComputeFallbackAuditManifest(FallbackAuditManifest item)
    {
        return PolicyAckNoticePolicy.PolicyAckNoticeWeight(6, true);
    }
}
