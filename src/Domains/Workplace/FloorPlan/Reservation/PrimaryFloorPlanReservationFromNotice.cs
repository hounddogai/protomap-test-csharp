namespace Acme.Workforce.Workplace.FloorPlan.Reservation;

using Acme.Workforce.Workplace.Booking.Digest;

/// <summary>Functions of primary floor plan reservation from notice.</summary>
public static class PrimaryFloorPlanReservationFromNotice
{
    public static Dictionary<string, double> SummarizeBookingNoticesForReservation(List<BookingNotice> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedTotal();
        }
        return totals;
    }

    public static List<PrimaryFloorPlanReservation> DerivePrimaryFloorPlanReservationsFromNotice(List<BookingNotice> items)
    {
        var outValue = new List<PrimaryFloorPlanReservation>();
        foreach (var item in items)
        {
            var derived = PrimaryFloorPlanReservation.NewPrimaryFloorPlanReservation(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ComputePrimaryFloorPlanReservation(PrimaryFloorPlanReservation item)
    {
        return BookingNoticePolicy.BookingNoticeWeight(5, true);
    }
}
