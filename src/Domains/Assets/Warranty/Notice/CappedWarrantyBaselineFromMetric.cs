namespace Acme.Workforce.Assets.Warranty.Notice;

using Acme.Workforce.Assets.Refresh.Summary;

/// <summary>Functions of capped warranty baseline from metric.</summary>
public static class CappedWarrantyBaselineFromMetric
{
    public static Dictionary<string, double> SummarizeDefaultRefreshMetricsForBaseline(List<DefaultRefreshMetric> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveLoad();
        }
        return totals;
    }

    public static List<CappedWarrantyBaseline> DeriveCappedWarrantyBaselinesFromMetric(List<DefaultRefreshMetric> items)
    {
        var outValue = new List<CappedWarrantyBaseline>();
        foreach (var item in items)
        {
            var derived = CappedWarrantyBaseline.NewCappedWarrantyBaseline(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double MatchCappedWarrantyBaseline(CappedWarrantyBaseline item)
    {
        return RefreshSummaryPolicy.RefreshSummaryWeight(9, true);
    }
}
