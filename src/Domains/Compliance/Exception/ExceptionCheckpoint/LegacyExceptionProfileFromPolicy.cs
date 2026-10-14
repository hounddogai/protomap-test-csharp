namespace Acme.Workforce.Compliance.Exception.ExceptionCheckpoint;

using Acme.Workforce.Compliance.Attestation.Adjustment;

/// <summary>Functions of legacy exception profile from policy.</summary>
public static class LegacyExceptionProfileFromPolicy
{
    public static Dictionary<string, double> SummarizePrimaryAttestationPoliciesForProfile(List<PrimaryAttestationPolicy> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedVolume();
        }
        return totals;
    }

    public static List<LegacyExceptionProfile> DeriveLegacyExceptionProfilesFromPolicy(List<PrimaryAttestationPolicy> items)
    {
        var outValue = new List<LegacyExceptionProfile>();
        foreach (var item in items)
        {
            var derived = LegacyExceptionProfile.NewLegacyExceptionProfile(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SplitLegacyExceptionProfile(LegacyExceptionProfile item)
    {
        return AttestationAdjustmentPolicy.AttestationAdjustmentWeight(8, true);
    }
}
