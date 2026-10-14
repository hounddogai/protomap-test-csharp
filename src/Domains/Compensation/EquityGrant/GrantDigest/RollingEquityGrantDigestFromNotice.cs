namespace Acme.Workforce.Compensation.EquityGrant.GrantDigest;

using Acme.Workforce.Compensation.Adjustment.Estimate;

/// <summary>Functions of rolling equity grant digest from notice.</summary>
public static class RollingEquityGrantDigestFromNotice
{
    public static Dictionary<string, double> SummarizeWeightedAdjustmentNoticesForDigest(List<WeightedAdjustmentNotice> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.MeasuredValue();
        }
        return totals;
    }

    public static List<RollingEquityGrantDigest> DeriveRollingEquityGrantDigestsFromNotice(List<WeightedAdjustmentNotice> items)
    {
        var outValue = new List<RollingEquityGrantDigest>();
        foreach (var item in items)
        {
            var derived = RollingEquityGrantDigest.NewRollingEquityGrantDigest(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizeRollingEquityGrantDigest(RollingEquityGrantDigest item)
    {
        return PendingAdjustmentEstimatePolicy.PendingAdjustmentEstimateBucket(item.AdjustedVolume());
    }
}
