namespace Acme.Workforce.Orgdesign.Reorg.Claim;

using Acme.Workforce.Orgdesign.Position.Adjustment;

/// <summary>Functions of reorg rollup from metric.</summary>
public static class ReorgRollupFromMetric
{
    public static Dictionary<string, double> SummarizeRegionalPositionMetricsForRollup(List<RegionalPositionMetric> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveExposure();
        }
        return totals;
    }

    public static List<ReorgRollup> DeriveReorgRollupsFromMetric(List<RegionalPositionMetric> items)
    {
        var outValue = new List<ReorgRollup>();
        foreach (var item in items)
        {
            var derived = ReorgRollup.NewReorgRollup(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SplitReorgRollup(ReorgRollup item)
    {
        return RegionalPositionMetricPolicy.RegionalPositionMetricBucket(item.AdjustedLoad());
    }
}
