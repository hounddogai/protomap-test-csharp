namespace Acme.Workforce.Compliance.Attestation.Rule;

using Acme.Workforce.Compliance.Audit.Bundle;

/// <summary>Functions of attestation rule from bundle.</summary>
public static class AttestationRuleFromBundle
{
    public static Dictionary<string, double> SummarizeAuditBundlesForRule(List<AuditBundle> items)
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

    public static List<AttestationRule> DeriveAttestationRulesFromBundle(List<AuditBundle> items)
    {
        var outValue = new List<AttestationRule>();
        foreach (var item in items)
        {
            var derived = AttestationRule.NewAttestationRule(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SettleAttestationRule(AttestationRule item)
    {
        return AuditEntryPolicy.ClassifyAuditEntry(item.EffectiveExposure());
    }
}
