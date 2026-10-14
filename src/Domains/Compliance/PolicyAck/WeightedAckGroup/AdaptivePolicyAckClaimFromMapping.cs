namespace Acme.Workforce.Compliance.PolicyAck.WeightedAckGroup;

using Acme.Workforce.Compliance.Attestation.Preference;

/// <summary>Functions of adaptive policy ack claim from mapping.</summary>
public static class AdaptivePolicyAckClaimFromMapping
{
    public static Dictionary<string, double> SummarizeSeasonalAttestationMappingsForClaim(List<SeasonalAttestationMapping> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveTotal();
        }
        return totals;
    }

    public static List<AdaptivePolicyAckClaim> DeriveAdaptivePolicyAckClaimsFromMapping(List<SeasonalAttestationMapping> items)
    {
        var outValue = new List<AdaptivePolicyAckClaim>();
        foreach (var item in items)
        {
            var derived = AdaptivePolicyAckClaim.NewAdaptivePolicyAckClaim(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizeAdaptivePolicyAckClaim(AdaptivePolicyAckClaim item)
    {
        return RegionalAttestationPreferencePolicy.ClassifyRegionalAttestationPreference(item.WeightedLoad());
    }
}
