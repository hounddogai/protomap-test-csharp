namespace Acme.Workforce.Workplace.AccessBadge.Bundle;

using Acme.Workforce.Workplace.AccessBadge.Audit;

/// <summary>Functions of tiered access badge bundle from batch.</summary>
public static class TieredAccessBadgeBundleFromBatch
{
    public static Dictionary<string, double> SummarizeRollingAccessBadgeBatchesForBundle(List<RollingAccessBadgeBatch> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedValue();
        }
        return totals;
    }

    public static List<TieredAccessBadgeBundle> DeriveTieredAccessBadgeBundlesFromBatch(List<RollingAccessBadgeBatch> items)
    {
        var outValue = new List<TieredAccessBadgeBundle>();
        foreach (var item in items)
        {
            var derived = TieredAccessBadgeBundle.NewTieredAccessBadgeBundle(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolveTieredAccessBadgeBundle(TieredAccessBadgeBundle item)
    {
        return RollingAccessBadgeBatchPolicy.RollingAccessBadgeBatchBucket(item.WeightedTotal());
    }
}
