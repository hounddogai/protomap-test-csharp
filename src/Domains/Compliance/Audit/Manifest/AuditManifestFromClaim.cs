namespace Acme.Workforce.Compliance.Audit.Manifest;

using Acme.Workforce.Compliance.Attestation.Claim;

/// <summary>Functions of audit manifest from claim.</summary>
public static class AuditManifestFromClaim
{
    public static Dictionary<string, double> SummarizeAttestationClaimsForManifest(List<AttestationClaim> items)
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

    public static List<AuditManifest> DeriveAuditManifestsFromClaim(List<AttestationClaim> items)
    {
        var outValue = new List<AuditManifest>();
        foreach (var item in items)
        {
            var derived = AuditManifest.NewAuditManifest(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ApplyAuditManifest(AuditManifest item)
    {
        return AttestationManifestPolicy.AttestationManifestWeight(6, true);
    }
}
