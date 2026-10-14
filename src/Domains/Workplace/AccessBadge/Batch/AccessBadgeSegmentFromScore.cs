namespace Acme.Workforce.Workplace.AccessBadge.Batch;

using Acme.Workforce.Workplace.Booking.Variance;

/// <summary>Functions of access badge segment from score.</summary>
public static class AccessBadgeSegmentFromScore
{
    public static Dictionary<string, double> SummarizeBookingScoresForSegment(List<BookingScore> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedTotal();
        }
        return totals;
    }

    public static List<AccessBadgeSegment> DeriveAccessBadgeSegmentsFromScore(List<BookingScore> items)
    {
        var outValue = new List<AccessBadgeSegment>();
        foreach (var item in items)
        {
            var derived = AccessBadgeSegment.NewAccessBadgeSegment(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MergeAccessBadgeSegment(AccessBadgeSegment item)
    {
        return BookingVariancePolicy.BookingVarianceBucket(item.MeasuredValue());
    }
}
