namespace Acme.Workforce.Compliance.PolicyAck.OverrideItem;

using Acme.Workforce.Compliance.Audit.Assignment;

/// <summary>Functions of policy ack exception from checkpoint.</summary>
public static class PolicyAckExceptionFromCheckpoint
{
    public static Dictionary<string, double> SummarizeGlobalAuditCheckpointsForException(List<GlobalAuditCheckpoint> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedLoad();
        }
        return totals;
    }

    public static List<PolicyAckException> DerivePolicyAckExceptionsFromCheckpoint(List<GlobalAuditCheckpoint> items)
    {
        var outValue = new List<PolicyAckException>();
        foreach (var item in items)
        {
            var derived = PolicyAckException.NewPolicyAckException(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ComputePolicyAckException(PolicyAckException item)
    {
        return GlobalAuditCheckpointPolicy.GlobalAuditCheckpointWeight(6, true);
    }
}
