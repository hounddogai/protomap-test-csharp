namespace Acme.Workforce.Compliance.PolicyAck.Profile;

using Acme.Workforce.Compliance.PolicyAck.Entry;

/// <summary>Functions of policy ack item from allocation.</summary>
public static class PolicyAckItemFromAllocation
{
    public static Dictionary<string, double> SummarizePolicyAckAllocationsForItem(List<PolicyAckAllocation> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedTotal();
        }
        return totals;
    }

    public static List<PolicyAckItem> DerivePolicyAckItemsFromAllocation(List<PolicyAckAllocation> items)
    {
        var outValue = new List<PolicyAckItem>();
        foreach (var item in items)
        {
            var derived = PolicyAckItem.NewPolicyAckItem(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ApplyPolicyAckItem(PolicyAckItem item)
    {
        return FallbackPolicyAckRequestPolicy.FallbackPolicyAckRequestWeight(4, true);
    }
}
