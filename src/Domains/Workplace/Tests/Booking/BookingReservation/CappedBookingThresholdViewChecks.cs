namespace Acme.Workforce.Workplace.Tests.Booking.BookingReservation;

using Acme.Workforce.Workplace.Booking.BookingReservation;

/// <summary>Checks of booking reservation code, run by the project's runner.</summary>
public static class CappedBookingThresholdViewChecks
{
    public static void CappedBookingThresholdViewShowsTheStatusInCapitals()
    {
        var item = CappedBookingThreshold.NewCappedBookingThreshold("code-643", "name-632");
        var view = CappedBookingThresholdView.ToCappedBookingThresholdView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CappedBookingThresholdView.ToCappedBookingThresholdViews(new List<CappedBookingThreshold> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CappedBookingThresholdViewShowsTheStatusInCapitals();
    }
}
