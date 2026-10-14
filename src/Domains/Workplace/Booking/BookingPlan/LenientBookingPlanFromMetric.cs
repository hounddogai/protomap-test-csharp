namespace Acme.Workforce.Workplace.Booking.BookingPlan;

using Acme.Workforce.Workplace.FloorPlan.Cycle;

/// <summary>Functions of lenient booking plan from metric.</summary>
public static class LenientBookingPlanFromMetric
{
    public static Dictionary<string, double> SummarizeAdaptiveFloorPlanMetricsForPlan(List<AdaptiveFloorPlanMetric> items)
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

    public static List<LenientBookingPlan> DeriveLenientBookingPlansFromMetric(List<AdaptiveFloorPlanMetric> items)
    {
        var outValue = new List<LenientBookingPlan>();
        foreach (var item in items)
        {
            var derived = LenientBookingPlan.NewLenientBookingPlan(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ClassifyLenientBookingPlan(LenientBookingPlan item)
    {
        return FloorPlanNoticePolicy.FloorPlanNoticeBucket(item.WeightedTotal());
    }
}
