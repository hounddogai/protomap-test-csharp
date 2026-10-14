namespace Acme.Workforce.Compliance.Attestation.Quota;

using Acme.Workforce.Compliance.PolicyAck.Profile;

/// <summary>Functions of attestation summary from profile.</summary>
public static class AttestationSummaryFromProfile
{
    public static Dictionary<string, double> SummarizeFallbackPolicyAckProfilesForSummary(List<FallbackPolicyAckProfile> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveLoad();
        }
        return totals;
    }

    public static List<AttestationSummary> DeriveAttestationSummariesFromProfile(List<FallbackPolicyAckProfile> items)
    {
        var outValue = new List<AttestationSummary>();
        foreach (var item in items)
        {
            var derived = AttestationSummary.NewAttestationSummary(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectAttestationSummary(AttestationSummary item)
    {
        return FallbackPolicyAckProfilePolicy.NormalizeFallbackPolicyAckProfileCode(item.Code);
    }
}
