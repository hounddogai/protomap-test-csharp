namespace Acme.Workforce.Assets.Warranty.Allocation;

using Acme.Workforce.Assets.Laptop.Adjustment;

/// <summary>Functions of adaptive warranty milestone from rollup.</summary>
public static class AdaptiveWarrantyMilestoneFromRollup
{
    public static Dictionary<string, double> SummarizeCappedLaptopRollupsForMilestone(List<CappedLaptopRollup> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedTotal();
        }
        return totals;
    }

    public static List<AdaptiveWarrantyMilestone> DeriveAdaptiveWarrantyMilestonesFromRollup(List<CappedLaptopRollup> items)
    {
        var outValue = new List<AdaptiveWarrantyMilestone>();
        foreach (var item in items)
        {
            var derived = AdaptiveWarrantyMilestone.NewAdaptiveWarrantyMilestone(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ResolveAdaptiveWarrantyMilestone(AdaptiveWarrantyMilestone item)
    {
        return CappedLaptopRollupPolicy.CappedLaptopRollupWeight(2, true);
    }
}
