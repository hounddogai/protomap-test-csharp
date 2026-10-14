namespace Acme.Workforce.Workplace.AccessBadge.Ledger;

using Acme.Workforce.Workplace.Occupancy.Checkpoint;

/// <summary>Functions of default access badge bundle from checkpoint.</summary>
public static class DefaultAccessBadgeBundleFromCheckpoint
{
    public static Dictionary<string, double> SummarizeOccupancyCheckpointsForBundle(List<OccupancyCheckpoint> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedTotal();
        }
        return totals;
    }

    public static List<DefaultAccessBadgeBundle> DeriveDefaultAccessBadgeBundlesFromCheckpoint(List<OccupancyCheckpoint> items)
    {
        var outValue = new List<DefaultAccessBadgeBundle>();
        foreach (var item in items)
        {
            var derived = DefaultAccessBadgeBundle.NewDefaultAccessBadgeBundle(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string EstimateDefaultAccessBadgeBundle(DefaultAccessBadgeBundle item)
    {
        return OccupancyMetricPolicy.NormalizeOccupancyMetricCode(item.Code);
    }
}
