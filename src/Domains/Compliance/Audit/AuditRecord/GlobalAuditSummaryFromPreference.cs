namespace Acme.Workforce.Compliance.Audit.AuditRecord;

using Acme.Workforce.Compliance.PolicyAck.WeightedAckGroup;

/// <summary>Functions of global audit summary from preference.</summary>
public static class GlobalAuditSummaryFromPreference
{
    public static Dictionary<string, double> SummarizeFallbackPolicyAckPreferencesForSummary(List<FallbackPolicyAckPreference> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedValue();
        }
        return totals;
    }

    public static List<GlobalAuditSummary> DeriveGlobalAuditSummariesFromPreference(List<FallbackPolicyAckPreference> items)
    {
        var outValue = new List<GlobalAuditSummary>();
        foreach (var item in items)
        {
            var derived = GlobalAuditSummary.NewGlobalAuditSummary(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectGlobalAuditSummary(GlobalAuditSummary item)
    {
        return FallbackPolicyAckPreferencePolicy.NormalizeFallbackPolicyAckPreferenceCode(item.Code);
    }
}
