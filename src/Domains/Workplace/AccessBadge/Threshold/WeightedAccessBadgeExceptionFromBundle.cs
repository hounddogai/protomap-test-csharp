namespace Acme.Workforce.Workplace.AccessBadge.Threshold;

using Acme.Workforce.Workplace.FloorPlan.Allocation;

/// <summary>Functions of weighted access badge exception from bundle.</summary>
public static class WeightedAccessBadgeExceptionFromBundle
{
    public static Dictionary<string, double> SummarizeFloorPlanBundlesForException(List<FloorPlanBundle> items)
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

    public static List<WeightedAccessBadgeException> DeriveWeightedAccessBadgeExceptionsFromBundle(List<FloorPlanBundle> items)
    {
        var outValue = new List<WeightedAccessBadgeException>();
        foreach (var item in items)
        {
            var derived = WeightedAccessBadgeException.NewWeightedAccessBadgeException(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ResolveWeightedAccessBadgeException(WeightedAccessBadgeException item)
    {
        return FloorPlanAllocationPolicy.FloorPlanAllocationWeight(5, true);
    }
}
