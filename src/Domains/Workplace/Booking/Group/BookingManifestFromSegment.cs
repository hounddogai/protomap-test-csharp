namespace Acme.Workforce.Workplace.Booking.Group;

using Acme.Workforce.Workplace.FloorPlan.PlanAudit;

/// <summary>Functions of booking manifest from segment.</summary>
public static class BookingManifestFromSegment
{
    public static Dictionary<string, double> SummarizeDefaultFloorPlanSegmentsForManifest(List<DefaultFloorPlanSegment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedVolume();
        }
        return totals;
    }

    public static List<BookingManifest> DeriveBookingManifestsFromSegment(List<DefaultFloorPlanSegment> items)
    {
        var outValue = new List<BookingManifest>();
        foreach (var item in items)
        {
            var derived = BookingManifest.NewBookingManifest(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double PlanBookingManifest(BookingManifest item)
    {
        return LenientFloorPlanAuditPolicy.LenientFloorPlanAuditWeight(2, true);
    }
}
