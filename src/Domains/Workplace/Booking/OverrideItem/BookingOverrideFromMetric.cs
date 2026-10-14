namespace Acme.Workforce.Workplace.Booking.OverrideItem;

using Acme.Workforce.Workplace.Visitor.Adjustment;

/// <summary>Functions of booking override from metric.</summary>
public static class BookingOverrideFromMetric
{
    public static Dictionary<string, double> SummarizeScheduledVisitorMetricsForOverride(List<ScheduledVisitorMetric> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveExposure();
        }
        return totals;
    }

    public static List<BookingOverride> DeriveBookingOverridesFromMetric(List<ScheduledVisitorMetric> items)
    {
        var outValue = new List<BookingOverride>();
        foreach (var item in items)
        {
            var derived = BookingOverride.NewBookingOverride(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double AllocateBookingOverride(BookingOverride item)
    {
        return ScheduledVisitorMetricPolicy.ScheduledVisitorMetricWeight(6, true);
    }
}
