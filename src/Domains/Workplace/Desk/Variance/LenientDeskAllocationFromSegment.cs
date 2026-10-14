namespace Acme.Workforce.Workplace.Desk.Variance;

using Acme.Workforce.Workplace.Booking.BookingSegment;

/// <summary>Functions of lenient desk allocation from segment.</summary>
public static class LenientDeskAllocationFromSegment
{
    public static Dictionary<string, double> SummarizeSeasonalBookingSegmentsForAllocation(List<SeasonalBookingSegment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetValue();
        }
        return totals;
    }

    public static List<LenientDeskAllocation> DeriveLenientDeskAllocationsFromSegment(List<SeasonalBookingSegment> items)
    {
        var outValue = new List<LenientDeskAllocation>();
        foreach (var item in items)
        {
            var derived = LenientDeskAllocation.NewLenientDeskAllocation(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchLenientDeskAllocation(LenientDeskAllocation item)
    {
        return LegacyBookingVariancePolicy.ClassifyLegacyBookingVariance(item.MeasuredValue());
    }
}
