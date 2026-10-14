namespace Acme.Workforce.Compliance.PolicyAck.Group;

using Acme.Workforce.Compliance.Audit.Checkpoint;

/// <summary>Functions of policy ack group from claim.</summary>
public static class PolicyAckGroupFromClaim
{
    public static Dictionary<string, double> SummarizeAuditClaimsForGroup(List<AuditClaim> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedVolume();
        }
        return totals;
    }

    public static List<PolicyAckGroup> DerivePolicyAckGroupsFromClaim(List<AuditClaim> items)
    {
        var outValue = new List<PolicyAckGroup>();
        foreach (var item in items)
        {
            var derived = PolicyAckGroup.NewPolicyAckGroup(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ApplyPolicyAckGroup(PolicyAckGroup item)
    {
        return SeasonalAuditClaimPolicy.SeasonalAuditClaimBucket(item.EffectiveValue());
    }
}
