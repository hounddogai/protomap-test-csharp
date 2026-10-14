namespace Acme.Workforce.Contractors.Rate.Notice;

using Acme.Workforce.Contractors.Rate.Milestone;

/// <summary>Functions of weighted rate notice from score.</summary>
public static class WeightedRateNoticeFromScore
{
    public static Dictionary<string, double> SummarizeRateScoresForNotice(List<RateScore> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetExposure();
        }
        return totals;
    }

    public static List<WeightedRateNotice> DeriveWeightedRateNoticesFromScore(List<RateScore> items)
    {
        var outValue = new List<WeightedRateNotice>();
        foreach (var item in items)
        {
            var derived = WeightedRateNotice.NewWeightedRateNotice(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizeWeightedRateNotice(WeightedRateNotice item)
    {
        return CappedRateMilestonePolicy.CappedRateMilestoneBucket(item.WeightedTotal());
    }
}
