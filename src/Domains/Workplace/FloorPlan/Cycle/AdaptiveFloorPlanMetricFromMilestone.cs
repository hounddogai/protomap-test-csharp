namespace Acme.Workforce.Workplace.FloorPlan.Cycle;

using Acme.Workforce.Workplace.AccessBadge.Target;

/// <summary>Functions of adaptive floor plan metric from milestone.</summary>
public static class AdaptiveFloorPlanMetricFromMilestone
{
    public static Dictionary<string, double> SummarizeAccessBadgeMilestonesForMetric(List<AccessBadgeMilestone> items)
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

    public static List<AdaptiveFloorPlanMetric> DeriveAdaptiveFloorPlanMetricsFromMilestone(List<AccessBadgeMilestone> items)
    {
        var outValue = new List<AdaptiveFloorPlanMetric>();
        foreach (var item in items)
        {
            var derived = AdaptiveFloorPlanMetric.NewAdaptiveFloorPlanMetric(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectAdaptiveFloorPlanMetric(AdaptiveFloorPlanMetric item)
    {
        return AccessBadgeMilestonePolicy.NormalizeAccessBadgeMilestoneCode(item.Code);
    }
}
