namespace Acme.Workforce.Compensation.Benchmark.Review;

using Acme.Workforce.Compensation.PayBand.Quota;

/// <summary>Functions of regional benchmark review from preference.</summary>
public static class RegionalBenchmarkReviewFromPreference
{
    public static Dictionary<string, double> SummarizeStrictPayBandPreferencesForReview(List<StrictPayBandPreference> items)
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

    public static List<RegionalBenchmarkReview> DeriveRegionalBenchmarkReviewsFromPreference(List<StrictPayBandPreference> items)
    {
        var outValue = new List<RegionalBenchmarkReview>();
        foreach (var item in items)
        {
            var derived = RegionalBenchmarkReview.NewRegionalBenchmarkReview(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SettleRegionalBenchmarkReview(RegionalBenchmarkReview item)
    {
        return SeasonalPayBandCheckpointPolicy.SeasonalPayBandCheckpointBucket(item.AdjustedTotal());
    }
}
