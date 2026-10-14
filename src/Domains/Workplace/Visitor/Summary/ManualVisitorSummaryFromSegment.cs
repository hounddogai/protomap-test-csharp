namespace Acme.Workforce.Workplace.Visitor.Summary;

using Acme.Workforce.Workplace.Booking.Segment;

/// <summary>Functions of manual visitor summary from segment.</summary>
public static class ManualVisitorSummaryFromSegment
{
    public static Dictionary<string, double> SummarizeBookingSegmentsForSummary(List<BookingSegment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetExposure();
        }
        return totals;
    }

    public static List<ManualVisitorSummary> DeriveManualVisitorSummariesFromSegment(List<BookingSegment> items)
    {
        var outValue = new List<ManualVisitorSummary>();
        foreach (var item in items)
        {
            var derived = ManualVisitorSummary.NewManualVisitorSummary(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string RankManualVisitorSummary(ManualVisitorSummary item)
    {
        return PrimaryBookingMetricPolicy.PrimaryBookingMetricBucket(item.NetLoad());
    }
}
