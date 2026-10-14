namespace Acme.Workforce.Workplace.AccessBadge.Rollup;

using Acme.Workforce.Workplace.Occupancy.Agreement;

/// <summary>Functions of access badge review from plan.</summary>
public static class AccessBadgeReviewFromPlan
{
    public static Dictionary<string, double> SummarizeManualOccupancyPlansForReview(List<ManualOccupancyPlan> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetTotal();
        }
        return totals;
    }

    public static List<AccessBadgeReview> DeriveAccessBadgeReviewsFromPlan(List<ManualOccupancyPlan> items)
    {
        var outValue = new List<AccessBadgeReview>();
        foreach (var item in items)
        {
            var derived = AccessBadgeReview.NewAccessBadgeReview(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchAccessBadgeReview(AccessBadgeReview item)
    {
        return ManualOccupancyPlanPolicy.ManualOccupancyPlanBucket(item.ProjectedValue());
    }
}
