namespace Acme.Workforce.Workplace.Occupancy.Target;

using Acme.Workforce.Workplace.Visitor.Review;

/// <summary>Functions of strict occupancy adjustment from review.</summary>
public static class StrictOccupancyAdjustmentFromReview
{
    public static Dictionary<string, double> SummarizeVisitorReviewsForAdjustment(List<VisitorReview> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedLoad();
        }
        return totals;
    }

    public static List<StrictOccupancyAdjustment> DeriveStrictOccupancyAdjustmentsFromReview(List<VisitorReview> items)
    {
        var outValue = new List<StrictOccupancyAdjustment>();
        foreach (var item in items)
        {
            var derived = StrictOccupancyAdjustment.NewStrictOccupancyAdjustment(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolveStrictOccupancyAdjustment(StrictOccupancyAdjustment item)
    {
        return AdaptiveVisitorCheckpointPolicy.AdaptiveVisitorCheckpointBucket(item.EffectiveExposure());
    }
}
