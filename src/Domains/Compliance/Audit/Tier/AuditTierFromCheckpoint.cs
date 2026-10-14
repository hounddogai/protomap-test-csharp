namespace Acme.Workforce.Compliance.Audit.Tier;

using Acme.Workforce.Compliance.Certification.Checkpoint;

/// <summary>Functions of audit tier from checkpoint.</summary>
public static class AuditTierFromCheckpoint
{
    public static Dictionary<string, double> SummarizeFallbackCertificationCheckpointsForTier(List<FallbackCertificationCheckpoint> items)
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

    public static List<AuditTier> DeriveAuditTiersFromCheckpoint(List<FallbackCertificationCheckpoint> items)
    {
        var outValue = new List<AuditTier>();
        foreach (var item in items)
        {
            var derived = AuditTier.NewAuditTier(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AllocateAuditTier(AuditTier item)
    {
        return FallbackCertificationCheckpointPolicy.ClassifyFallbackCertificationCheckpoint(item.ProjectedLoad());
    }
}
