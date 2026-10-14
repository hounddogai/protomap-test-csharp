namespace Acme.Workforce.Workplace.AccessBadge.Notice;

using Acme.Workforce.Workplace.Booking.Digest;

/// <summary>Functions of regional access badge review from claim.</summary>
public static class RegionalAccessBadgeReviewFromClaim
{
    public static Dictionary<string, double> SummarizeDefaultBookingClaimsForReview(List<DefaultBookingClaim> items)
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

    public static List<RegionalAccessBadgeReview> DeriveRegionalAccessBadgeReviewsFromClaim(List<DefaultBookingClaim> items)
    {
        var outValue = new List<RegionalAccessBadgeReview>();
        foreach (var item in items)
        {
            var derived = RegionalAccessBadgeReview.NewRegionalAccessBadgeReview(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizeRegionalAccessBadgeReview(RegionalAccessBadgeReview item)
    {
        return DefaultBookingClaimPolicy.ClassifyDefaultBookingClaim(item.MeasuredValue());
    }
}
