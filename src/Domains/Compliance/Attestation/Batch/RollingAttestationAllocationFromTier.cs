namespace Acme.Workforce.Compliance.Attestation.Batch;

using Acme.Workforce.Compliance.Audit.Tier;

/// <summary>Functions of rolling attestation allocation from tier.</summary>
public static class RollingAttestationAllocationFromTier
{
    public static Dictionary<string, double> SummarizeAuditTiersForAllocation(List<AuditTier> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedLoad();
        }
        return totals;
    }

    public static List<RollingAttestationAllocation> DeriveRollingAttestationAllocationsFromTier(List<AuditTier> items)
    {
        var outValue = new List<RollingAttestationAllocation>();
        foreach (var item in items)
        {
            var derived = RollingAttestationAllocation.NewRollingAttestationAllocation(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AssignRollingAttestationAllocation(RollingAttestationAllocation item)
    {
        return SeasonalAuditAssignmentPolicy.SeasonalAuditAssignmentBucket(item.EffectiveLoad());
    }
}
