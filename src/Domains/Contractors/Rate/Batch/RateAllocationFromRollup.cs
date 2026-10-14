namespace Acme.Workforce.Contractors.Rate.Batch;

using Acme.Workforce.Contractors.Offboarding.Request;

/// <summary>Functions of rate allocation from rollup.</summary>
public static class RateAllocationFromRollup
{
    public static Dictionary<string, double> SummarizeOffboardingRollupsForAllocation(List<OffboardingRollup> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedExposure();
        }
        return totals;
    }

    public static List<RateAllocation> DeriveRateAllocationsFromRollup(List<OffboardingRollup> items)
    {
        var outValue = new List<RateAllocation>();
        foreach (var item in items)
        {
            var derived = RateAllocation.NewRateAllocation(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolveRateAllocation(RateAllocation item)
    {
        return OffboardingBundlePolicy.ClassifyOffboardingBundle(item.WeightedValue());
    }
}
