namespace Acme.Workforce.Workplace.AccessBadge.Ledger;

using Acme.Workforce.Workplace.FloorPlan.Audit;

/// <summary>Functions of default access badge bundle from segment.</summary>
public static class DefaultAccessBadgeBundleFromSegment
{
    public static Dictionary<string, double> SummarizeFloorPlanSegmentsForBundle(List<FloorPlanSegment> items)
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

    public static List<DefaultAccessBadgeBundle> DeriveDefaultAccessBadgeBundlesFromSegment(List<FloorPlanSegment> items)
    {
        var outValue = new List<DefaultAccessBadgeBundle>();
        foreach (var item in items)
        {
            var derived = DefaultAccessBadgeBundle.NewDefaultAccessBadgeBundle(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double NormalizeDefaultAccessBadgeBundle(DefaultAccessBadgeBundle item)
    {
        return FloorPlanSegmentPolicy.FloorPlanSegmentWeight(4, true);
    }
}
