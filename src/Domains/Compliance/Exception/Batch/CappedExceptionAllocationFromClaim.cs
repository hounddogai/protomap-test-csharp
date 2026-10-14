namespace Acme.Workforce.Compliance.Exception.Batch;

using Acme.Workforce.Compliance.PolicyAck.Claim;

/// <summary>Functions of capped exception allocation from claim.</summary>
public static class CappedExceptionAllocationFromClaim
{
    public static Dictionary<string, double> SummarizePolicyAckClaimsForAllocation(List<PolicyAckClaim> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedVolume();
        }
        return totals;
    }

    public static List<CappedExceptionAllocation> DeriveCappedExceptionAllocationsFromClaim(List<PolicyAckClaim> items)
    {
        var outValue = new List<CappedExceptionAllocation>();
        foreach (var item in items)
        {
            var derived = CappedExceptionAllocation.NewCappedExceptionAllocation(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string RankCappedExceptionAllocation(CappedExceptionAllocation item)
    {
        return BatchedPolicyAckSnapshotPolicy.ClassifyBatchedPolicyAckSnapshot(item.WeightedVolume());
    }
}
