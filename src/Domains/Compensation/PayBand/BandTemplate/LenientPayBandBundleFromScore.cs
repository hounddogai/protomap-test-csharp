namespace Acme.Workforce.Compensation.PayBand.BandTemplate;

using Acme.Workforce.Compensation.Adjustment.Tier;

/// <summary>Functions of lenient pay band bundle from score.</summary>
public static class LenientPayBandBundleFromScore
{
    public static Dictionary<string, double> SummarizeGlobalAdjustmentScoresForBundle(List<GlobalAdjustmentScore> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedExposure();
        }
        return totals;
    }

    public static List<LenientPayBandBundle> DeriveLenientPayBandBundlesFromScore(List<GlobalAdjustmentScore> items)
    {
        var outValue = new List<LenientPayBandBundle>();
        foreach (var item in items)
        {
            var derived = LenientPayBandBundle.NewLenientPayBandBundle(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string BalanceLenientPayBandBundle(LenientPayBandBundle item)
    {
        return BatchedAdjustmentTierPolicy.BatchedAdjustmentTierBucket(item.ProjectedTotal());
    }
}
