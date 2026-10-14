namespace Acme.Workforce.Orgdesign.Position.PositionAudit;

using Acme.Workforce.Orgdesign.Reorg.ReorgPlan;

/// <summary>Functions of weighted position forecast from estimate.</summary>
public static class WeightedPositionForecastFromEstimate
{
    public static Dictionary<string, double> SummarizeStrictReorgEstimatesForForecast(List<StrictReorgEstimate> items)
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

    public static List<WeightedPositionForecast> DeriveWeightedPositionForecastsFromEstimate(List<StrictReorgEstimate> items)
    {
        var outValue = new List<WeightedPositionForecast>();
        foreach (var item in items)
        {
            var derived = WeightedPositionForecast.NewWeightedPositionForecast(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AssignWeightedPositionForecast(WeightedPositionForecast item)
    {
        return ManualReorgVariancePolicy.ManualReorgVarianceBucket(item.ProjectedVolume());
    }
}
