namespace Acme.Workforce.Workplace.AccessBadge.Notice;

using Acme.Workforce.Workplace.Occupancy.Reservation;

/// <summary>Functions of regional access badge review from bundle.</summary>
public static class RegionalAccessBadgeReviewFromBundle
{
    public static Dictionary<string, double> SummarizeOccupancyBundlesForReview(List<OccupancyBundle> items)
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

    public static List<RegionalAccessBadgeReview> DeriveRegionalAccessBadgeReviewsFromBundle(List<OccupancyBundle> items)
    {
        var outValue = new List<RegionalAccessBadgeReview>();
        foreach (var item in items)
        {
            var derived = RegionalAccessBadgeReview.NewRegionalAccessBadgeReview(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ComputeRegionalAccessBadgeReview(RegionalAccessBadgeReview item)
    {
        return OccupancyReservationPolicy.OccupancyReservationWeight(3, true);
    }
}
