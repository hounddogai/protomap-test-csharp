namespace Acme.Workforce.Orgdesign.Scenario.Snapshot;

using Acme.Workforce.Orgdesign.Headcount.Rollup;

/// <summary>Functions of regional scenario batch from metric.</summary>
public static class RegionalScenarioBatchFromMetric
{
    public static Dictionary<string, double> SummarizeHeadcountMetricsForBatch(List<HeadcountMetric> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedLoad();
        }
        return totals;
    }

    public static List<RegionalScenarioBatch> DeriveRegionalScenarioBatchesFromMetric(List<HeadcountMetric> items)
    {
        var outValue = new List<RegionalScenarioBatch>();
        foreach (var item in items)
        {
            var derived = RegionalScenarioBatch.NewRegionalScenarioBatch(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ProjectRegionalScenarioBatch(RegionalScenarioBatch item)
    {
        return WeightedHeadcountRollupPolicy.WeightedHeadcountRollupWeight(8, true);
    }
}
