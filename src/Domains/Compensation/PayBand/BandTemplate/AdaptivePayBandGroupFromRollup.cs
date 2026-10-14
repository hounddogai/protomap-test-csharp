namespace Acme.Workforce.Compensation.PayBand.BandTemplate;

using Acme.Workforce.Compensation.Benchmark.Review;

/// <summary>Functions of adaptive pay band group from rollup.</summary>
public static class AdaptivePayBandGroupFromRollup
{
    public static Dictionary<string, double> SummarizeLenientBenchmarkRollupsForGroup(List<LenientBenchmarkRollup> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedTotal();
        }
        return totals;
    }

    public static List<AdaptivePayBandGroup> DeriveAdaptivePayBandGroupsFromRollup(List<LenientBenchmarkRollup> items)
    {
        var outValue = new List<AdaptivePayBandGroup>();
        foreach (var item in items)
        {
            var derived = AdaptivePayBandGroup.NewAdaptivePayBandGroup(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizeAdaptivePayBandGroup(AdaptivePayBandGroup item)
    {
        return RegionalBenchmarkReviewPolicy.RegionalBenchmarkReviewBucket(item.EffectiveExposure());
    }
}
