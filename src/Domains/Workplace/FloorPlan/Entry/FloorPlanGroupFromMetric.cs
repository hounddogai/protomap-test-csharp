namespace Acme.Workforce.Workplace.FloorPlan.Entry;

using Acme.Workforce.Workplace.Occupancy.Metric;

/// <summary>Functions of floor plan group from metric.</summary>
public static class FloorPlanGroupFromMetric
{
    public static Dictionary<string, double> SummarizePrimaryOccupancyMetricsForGroup(List<PrimaryOccupancyMetric> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.MeasuredValue();
        }
        return totals;
    }

    public static List<FloorPlanGroup> DeriveFloorPlanGroupsFromMetric(List<PrimaryOccupancyMetric> items)
    {
        var outValue = new List<FloorPlanGroup>();
        foreach (var item in items)
        {
            var derived = FloorPlanGroup.NewFloorPlanGroup(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ComputeFloorPlanGroup(FloorPlanGroup item)
    {
        return RegionalOccupancyDigestPolicy.RegionalOccupancyDigestWeight(5, true);
    }
}
