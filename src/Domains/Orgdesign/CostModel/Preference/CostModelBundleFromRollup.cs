namespace Acme.Workforce.Orgdesign.CostModel.Preference;

using Acme.Workforce.Orgdesign.Span.Rollup;

/// <summary>Functions of cost model bundle from rollup.</summary>
public static class CostModelBundleFromRollup
{
    public static Dictionary<string, double> SummarizeSpanRollupsForBundle(List<SpanRollup> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedVolume();
        }
        return totals;
    }

    public static List<CostModelBundle> DeriveCostModelBundlesFromRollup(List<SpanRollup> items)
    {
        var outValue = new List<CostModelBundle>();
        foreach (var item in items)
        {
            var derived = CostModelBundle.NewCostModelBundle(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AllocateCostModelBundle(CostModelBundle item)
    {
        return SpanBaselinePolicy.SpanBaselineBucket(item.WeightedExposure());
    }
}
