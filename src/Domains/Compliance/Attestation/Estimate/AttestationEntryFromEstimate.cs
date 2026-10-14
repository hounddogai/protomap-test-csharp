namespace Acme.Workforce.Compliance.Attestation.Estimate;

using Acme.Workforce.Compliance.PolicyAck.Bundle;

/// <summary>Functions of attestation entry from estimate.</summary>
public static class AttestationEntryFromEstimate
{
    public static Dictionary<string, double> SummarizePolicyAckEstimatesForEntry(List<PolicyAckEstimate> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveExposure();
        }
        return totals;
    }

    public static List<AttestationEntry> DeriveAttestationEntriesFromEstimate(List<PolicyAckEstimate> items)
    {
        var outValue = new List<AttestationEntry>();
        foreach (var item in items)
        {
            var derived = AttestationEntry.NewAttestationEntry(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScoreAttestationEntry(AttestationEntry item)
    {
        return PolicyAckBundlePolicy.ClassifyPolicyAckBundle(item.ProjectedTotal());
    }
}
