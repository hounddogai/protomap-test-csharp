namespace Acme.Workforce.Compliance.Audit.Baseline;

using Acme.Workforce.Compliance.Attestation.AttestationSchedule;

/// <summary>Functions of audit baseline from claim.</summary>
public static class AuditBaselineFromClaim
{
    public static Dictionary<string, double> SummarizeFallbackAttestationClaimsForBaseline(List<FallbackAttestationClaim> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetLoad();
        }
        return totals;
    }

    public static List<AuditBaseline> DeriveAuditBaselinesFromClaim(List<FallbackAttestationClaim> items)
    {
        var outValue = new List<AuditBaseline>();
        foreach (var item in items)
        {
            var derived = AuditBaseline.NewAuditBaseline(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ApplyAuditBaseline(AuditBaseline item)
    {
        return FallbackAttestationClaimPolicy.NormalizeFallbackAttestationClaimCode(item.Code);
    }
}
