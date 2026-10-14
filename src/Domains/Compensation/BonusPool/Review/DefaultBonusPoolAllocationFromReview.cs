namespace Acme.Workforce.Compensation.BonusPool.Review;

using Acme.Workforce.Compensation.Adjustment.Schedule;

/// <summary>Functions of default bonus pool allocation from review.</summary>
public static class DefaultBonusPoolAllocationFromReview
{
    public static Dictionary<string, double> SummarizeAdjustmentReviewsForAllocation(List<AdjustmentReview> items)
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

    public static List<DefaultBonusPoolAllocation> DeriveDefaultBonusPoolAllocationsFromReview(List<AdjustmentReview> items)
    {
        var outValue = new List<DefaultBonusPoolAllocation>();
        foreach (var item in items)
        {
            var derived = DefaultBonusPoolAllocation.NewDefaultBonusPoolAllocation(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectDefaultBonusPoolAllocation(DefaultBonusPoolAllocation item)
    {
        return ScheduledAdjustmentReservationPolicy.ClassifyScheduledAdjustmentReservation(item.ProjectedValue());
    }
}
