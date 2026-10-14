namespace Acme.Workforce.Compensation.Benchmark.AdaptiveBenchmarkAdjustment;

using Acme.Workforce.Compensation.EquityGrant.OverrideItem;

/// <summary>Functions of adaptive benchmark notice from override.</summary>
public static class AdaptiveBenchmarkNoticeFromOverride
{
    public static Dictionary<string, double> SummarizeAdaptiveEquityGrantOverridesForNotice(List<AdaptiveEquityGrantOverride> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetTotal();
        }
        return totals;
    }

    public static List<AdaptiveBenchmarkNotice> DeriveAdaptiveBenchmarkNoticesFromOverride(List<AdaptiveEquityGrantOverride> items)
    {
        var outValue = new List<AdaptiveBenchmarkNotice>();
        foreach (var item in items)
        {
            var derived = AdaptiveBenchmarkNotice.NewAdaptiveBenchmarkNotice(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolveAdaptiveBenchmarkNotice(AdaptiveBenchmarkNotice item)
    {
        return AdaptiveEquityGrantOverridePolicy.NormalizeAdaptiveEquityGrantOverrideCode(item.Code);
    }
}
