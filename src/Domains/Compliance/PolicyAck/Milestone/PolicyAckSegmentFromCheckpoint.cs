namespace Acme.Workforce.Compliance.PolicyAck.Milestone;

using Acme.Workforce.Compliance.PolicyAck.AckQuota;

/// <summary>Functions of policy ack segment from checkpoint.</summary>
public static class PolicyAckSegmentFromCheckpoint
{
    public static Dictionary<string, double> SummarizePolicyAckCheckpointsForSegment(List<PolicyAckCheckpoint> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetExposure();
        }
        return totals;
    }

    public static List<PolicyAckSegment> DerivePolicyAckSegmentsFromCheckpoint(List<PolicyAckCheckpoint> items)
    {
        var outValue = new List<PolicyAckSegment>();
        foreach (var item in items)
        {
            var derived = PolicyAckSegment.NewPolicyAckSegment(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double MergePolicyAckSegment(PolicyAckSegment item)
    {
        return LegacyPolicyAckClaimPolicy.LegacyPolicyAckClaimWeight(8, true);
    }
}
