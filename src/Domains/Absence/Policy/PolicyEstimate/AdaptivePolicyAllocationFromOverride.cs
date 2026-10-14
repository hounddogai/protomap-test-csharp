namespace Acme.Workforce.Absence.Policy.PolicyEstimate;

using Acme.Workforce.Absence.Accrual.Digest;

/// <summary>Functions of adaptive policy allocation from override.</summary>
public static class AdaptivePolicyAllocationFromOverride
{
    public static Dictionary<string, double> SummarizeScheduledAccrualOverridesForAllocation(List<ScheduledAccrualOverride> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedTotal();
        }
        return totals;
    }

    public static List<AdaptivePolicyAllocation> DeriveAdaptivePolicyAllocationsFromOverride(List<ScheduledAccrualOverride> items)
    {
        var outValue = new List<AdaptivePolicyAllocation>();
        foreach (var item in items)
        {
            var derived = AdaptivePolicyAllocation.NewAdaptivePolicyAllocation(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchAdaptivePolicyAllocation(AdaptivePolicyAllocation item)
    {
        return SeasonalAccrualDigestPolicy.SeasonalAccrualDigestBucket(item.EffectiveLoad());
    }
}
