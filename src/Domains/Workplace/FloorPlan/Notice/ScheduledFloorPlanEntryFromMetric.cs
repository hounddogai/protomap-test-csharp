namespace Acme.Workforce.Workplace.FloorPlan.Notice;

using Acme.Workforce.Workplace.FloorPlan.Entry;

/// <summary>Functions of scheduled floor plan entry from metric.</summary>
public static class ScheduledFloorPlanEntryFromMetric
{
    public static Dictionary<string, double> SummarizeWeightedFloorPlanMetricsForEntry(List<WeightedFloorPlanMetric> items)
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

    public static List<ScheduledFloorPlanEntry> DeriveScheduledFloorPlanEntriesFromMetric(List<WeightedFloorPlanMetric> items)
    {
        var outValue = new List<ScheduledFloorPlanEntry>();
        foreach (var item in items)
        {
            var derived = ScheduledFloorPlanEntry.NewScheduledFloorPlanEntry(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizeScheduledFloorPlanEntry(ScheduledFloorPlanEntry item)
    {
        return FloorPlanEntryPolicy.ClassifyFloorPlanEntry(item.WeightedValue());
    }
}
