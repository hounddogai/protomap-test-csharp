namespace Acme.Workforce.Compliance.Certification.Checkpoint;

using Acme.Workforce.Compliance.Audit.Claim;

/// <summary>Functions of fallback certification checkpoint from profile.</summary>
public static class FallbackCertificationCheckpointFromProfile
{
    public static Dictionary<string, double> SummarizeAuditProfilesForCheckpoint(List<AuditProfile> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedLoad();
        }
        return totals;
    }

    public static List<FallbackCertificationCheckpoint> DeriveFallbackCertificationCheckpointsFromProfile(List<AuditProfile> items)
    {
        var outValue = new List<FallbackCertificationCheckpoint>();
        foreach (var item in items)
        {
            var derived = FallbackCertificationCheckpoint.NewFallbackCertificationCheckpoint(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizeFallbackCertificationCheckpoint(FallbackCertificationCheckpoint item)
    {
        return LegacyAuditClaimPolicy.ClassifyLegacyAuditClaim(item.ProjectedValue());
    }
}
