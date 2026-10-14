namespace Acme.Workforce.Workplace.Desk.DeskRecord;

using Acme.Workforce.Workplace.FloorPlan.Entry;

/// <summary>Functions of pending desk baseline from metric.</summary>
public static class PendingDeskBaselineFromMetric
{
    public static Dictionary<string, double> SummarizeWeightedFloorPlanMetricsForBaseline(List<WeightedFloorPlanMetric> items)
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

    public static List<PendingDeskBaseline> DerivePendingDeskBaselinesFromMetric(List<WeightedFloorPlanMetric> items)
    {
        var outValue = new List<PendingDeskBaseline>();
        foreach (var item in items)
        {
            var derived = PendingDeskBaseline.NewPendingDeskBaseline(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SplitPendingDeskBaseline(PendingDeskBaseline item)
    {
        return WeightedFloorPlanMetricPolicy.ClassifyWeightedFloorPlanMetric(item.WeightedLoad());
    }
}
