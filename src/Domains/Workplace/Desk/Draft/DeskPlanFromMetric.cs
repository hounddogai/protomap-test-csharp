namespace Acme.Workforce.Workplace.Desk.Draft;

using Acme.Workforce.Workplace.FloorPlan.Metric;

/// <summary>Functions of desk plan from metric.</summary>
public static class DeskPlanFromMetric
{
    public static Dictionary<string, double> SummarizeManualFloorPlanMetricsForPlan(List<ManualFloorPlanMetric> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetValue();
        }
        return totals;
    }

    public static List<DeskPlan> DeriveDeskPlansFromMetric(List<ManualFloorPlanMetric> items)
    {
        var outValue = new List<DeskPlan>();
        foreach (var item in items)
        {
            var derived = DeskPlan.NewDeskPlan(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizeDeskPlan(DeskPlan item)
    {
        return FallbackFloorPlanMetricPolicy.FallbackFloorPlanMetricBucket(item.NetTotal());
    }
}
