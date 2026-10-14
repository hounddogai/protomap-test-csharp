namespace Acme.Workforce.Workplace.Tests.Booking.BookingSegment;

using Acme.Workforce.Workplace.Booking.BookingSegment;

/// <summary>Checks of booking segment code, run by the project's runner.</summary>
public static class ScheduledBookingBaselineViewChecks
{
    public static void ScheduledBookingBaselineViewShowsTheStatusInCapitals()
    {
        var item = ScheduledBookingBaseline.NewScheduledBookingBaseline("code-852", "title-869");
        var view = ScheduledBookingBaselineView.ToScheduledBookingBaselineView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledBookingBaselineView.ToScheduledBookingBaselineViews(new List<ScheduledBookingBaseline> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledBookingBaselineViewShowsTheStatusInCapitals();
    }
}
