namespace Acme.Workforce.Orgdesign.Headcount.Allocation;

using Acme.Workforce.Orgdesign.Position.Estimate;

/// <summary>Functions of headcount allocation from policy.</summary>
public static class HeadcountAllocationFromPolicy
{
    public static Dictionary<string, double> SummarizeTieredPositionPoliciesForAllocation(List<TieredPositionPolicy> items)
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

    public static List<HeadcountAllocation> DeriveHeadcountAllocationsFromPolicy(List<TieredPositionPolicy> items)
    {
        var outValue = new List<HeadcountAllocation>();
        foreach (var item in items)
        {
            var derived = HeadcountAllocation.NewHeadcountAllocation(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ApplyHeadcountAllocation(HeadcountAllocation item)
    {
        return TieredPositionPolicyPolicy.TieredPositionPolicyBucket(item.NetValue());
    }
}
