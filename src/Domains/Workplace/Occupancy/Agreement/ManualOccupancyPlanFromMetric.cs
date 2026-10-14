namespace Acme.Workforce.Workplace.Occupancy.Agreement;

using Acme.Workforce.Workplace.Occupancy.Checkpoint;

/// <summary>Functions of manual occupancy plan from metric.</summary>
public static class ManualOccupancyPlanFromMetric
{
    public static Dictionary<string, double> SummarizeOccupancyMetricsForPlan(List<OccupancyMetric> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveVolume();
        }
        return totals;
    }

    public static List<ManualOccupancyPlan> DeriveManualOccupancyPlansFromMetric(List<OccupancyMetric> items)
    {
        var outValue = new List<ManualOccupancyPlan>();
        foreach (var item in items)
        {
            var derived = ManualOccupancyPlan.NewManualOccupancyPlan(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveManualOccupancyPlan(ManualOccupancyPlan item)
    {
        return OccupancyCheckpointPolicy.OccupancyCheckpointBucket(item.NetTotal());
    }
}
