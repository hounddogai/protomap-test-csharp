namespace Acme.Workforce.Orgdesign.Reorg.ReorgSnapshot;

using Acme.Workforce.Orgdesign.Span.Template;

/// <summary>Functions of tiered reorg variance from mapping.</summary>
public static class TieredReorgVarianceFromMapping
{
    public static Dictionary<string, double> SummarizeWeightedSpanMappingsForVariance(List<WeightedSpanMapping> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedLoad();
        }
        return totals;
    }

    public static List<TieredReorgVariance> DeriveTieredReorgVariancesFromMapping(List<WeightedSpanMapping> items)
    {
        var outValue = new List<TieredReorgVariance>();
        foreach (var item in items)
        {
            var derived = TieredReorgVariance.NewTieredReorgVariance(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AllocateTieredReorgVariance(TieredReorgVariance item)
    {
        return SpanBudgetPolicy.NormalizeSpanBudgetCode(item.Code);
    }
}
