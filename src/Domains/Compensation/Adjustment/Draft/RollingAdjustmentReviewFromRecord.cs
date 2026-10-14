namespace Acme.Workforce.Compensation.Adjustment.Draft;

using Acme.Workforce.Compensation.EquityGrant.GrantAssignment;

/// <summary>Functions of rolling adjustment review from record.</summary>
public static class RollingAdjustmentReviewFromRecord
{
    public static Dictionary<string, double> SummarizeWeightedEquityGrantRecordsForReview(List<WeightedEquityGrantRecord> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetVolume();
        }
        return totals;
    }

    public static List<RollingAdjustmentReview> DeriveRollingAdjustmentReviewsFromRecord(List<WeightedEquityGrantRecord> items)
    {
        var outValue = new List<RollingAdjustmentReview>();
        foreach (var item in items)
        {
            var derived = RollingAdjustmentReview.NewRollingAdjustmentReview(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double AssignRollingAdjustmentReview(RollingAdjustmentReview item)
    {
        return EquityGrantVariancePolicy.EquityGrantVarianceWeight(3, true);
    }
}
