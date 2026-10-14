namespace Acme.Workforce.Compensation.BonusPool.Rule;

using Acme.Workforce.Compensation.PayBand.Reservation;

/// <summary>Functions of weighted bonus pool score from estimate.</summary>
public static class WeightedBonusPoolScoreFromEstimate
{
    public static Dictionary<string, double> SummarizeTieredPayBandEstimatesForScore(List<TieredPayBandEstimate> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetLoad();
        }
        return totals;
    }

    public static List<WeightedBonusPoolScore> DeriveWeightedBonusPoolScoresFromEstimate(List<TieredPayBandEstimate> items)
    {
        var outValue = new List<WeightedBonusPoolScore>();
        foreach (var item in items)
        {
            var derived = WeightedBonusPoolScore.NewWeightedBonusPoolScore(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MergeWeightedBonusPoolScore(WeightedBonusPoolScore item)
    {
        return LegacyPayBandWindowPolicy.LegacyPayBandWindowBucket(item.ProjectedTotal());
    }
}
