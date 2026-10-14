namespace Acme.Workforce.Workplace.Desk.RecordItem;

using Acme.Workforce.Workplace.Occupancy.Checkpoint;

/// <summary>Functions of desk baseline from metric.</summary>
public static class DeskBaselineFromMetric
{
    public static Dictionary<string, double> SummarizeOccupancyMetricsForBaseline(List<OccupancyMetric> items)
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

    public static List<DeskBaseline> DeriveDeskBaselinesFromMetric(List<OccupancyMetric> items)
    {
        var outValue = new List<DeskBaseline>();
        foreach (var item in items)
        {
            var derived = DeskBaseline.NewDeskBaseline(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolveDeskBaseline(DeskBaseline item)
    {
        return OccupancyMetricPolicy.OccupancyMetricBucket(item.NetVolume());
    }
}
