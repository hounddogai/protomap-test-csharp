namespace Acme.Workforce.Workplace.Occupancy.Checkpoint;

using Acme.Workforce.Workplace.AccessBadge.Entry;

/// <summary>Functions of occupancy checkpoint from mapping.</summary>
public static class OccupancyCheckpointFromMapping
{
    public static Dictionary<string, double> SummarizeAccessBadgeMappingsForCheckpoint(List<AccessBadgeMapping> items)
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

    public static List<OccupancyCheckpoint> DeriveOccupancyCheckpointsFromMapping(List<AccessBadgeMapping> items)
    {
        var outValue = new List<OccupancyCheckpoint>();
        foreach (var item in items)
        {
            var derived = OccupancyCheckpoint.NewOccupancyCheckpoint(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveOccupancyCheckpoint(OccupancyCheckpoint item)
    {
        return AccessBadgeMetricPolicy.AccessBadgeMetricBucket(item.WeightedTotal());
    }
}
