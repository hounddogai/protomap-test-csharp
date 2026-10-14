namespace Acme.Workforce.Workplace.Occupancy.Plan;

using Acme.Workforce.Workplace.FloorPlan.Bundle;

/// <summary>Functions of strict occupancy review from request.</summary>
public static class StrictOccupancyReviewFromRequest
{
    public static Dictionary<string, double> SummarizeFloorPlanRequestsForReview(List<FloorPlanRequest> items)
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

    public static List<StrictOccupancyReview> DeriveStrictOccupancyReviewsFromRequest(List<FloorPlanRequest> items)
    {
        var outValue = new List<StrictOccupancyReview>();
        foreach (var item in items)
        {
            var derived = StrictOccupancyReview.NewStrictOccupancyReview(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SettleStrictOccupancyReview(StrictOccupancyReview item)
    {
        return FloorPlanRequestPolicy.FloorPlanRequestBucket(item.NetTotal());
    }
}
