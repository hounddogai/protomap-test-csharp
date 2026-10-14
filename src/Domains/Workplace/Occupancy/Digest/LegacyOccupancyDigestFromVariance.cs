namespace Acme.Workforce.Workplace.Occupancy.Digest;

using Acme.Workforce.Workplace.FloorPlan.Notice;

/// <summary>Functions of legacy occupancy digest from variance.</summary>
public static class LegacyOccupancyDigestFromVariance
{
    public static Dictionary<string, double> SummarizeFloorPlanVariancesForDigest(List<FloorPlanVariance> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedValue();
        }
        return totals;
    }

    public static List<LegacyOccupancyDigest> DeriveLegacyOccupancyDigestsFromVariance(List<FloorPlanVariance> items)
    {
        var outValue = new List<LegacyOccupancyDigest>();
        foreach (var item in items)
        {
            var derived = LegacyOccupancyDigest.NewLegacyOccupancyDigest(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolveLegacyOccupancyDigest(LegacyOccupancyDigest item)
    {
        return RollingFloorPlanSegmentPolicy.RollingFloorPlanSegmentBucket(item.ProjectedLoad());
    }
}
