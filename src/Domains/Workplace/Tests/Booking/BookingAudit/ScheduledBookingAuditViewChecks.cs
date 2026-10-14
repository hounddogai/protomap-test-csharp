namespace Acme.Workforce.Workplace.Tests.Booking.BookingAudit;

using Acme.Workforce.Workplace.Booking.BookingAudit;

/// <summary>Checks of booking audit code, run by the project's runner.</summary>
public static class ScheduledBookingAuditViewChecks
{
    public static void ScheduledBookingAuditViewShowsTheStatusInCapitals()
    {
        var item = ScheduledBookingAudit.NewScheduledBookingAudit("code-692", "title-203");
        var view = ScheduledBookingAuditView.ToScheduledBookingAuditView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledBookingAuditView.ToScheduledBookingAuditViews(new List<ScheduledBookingAudit> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledBookingAuditViewShowsTheStatusInCapitals();
    }
}
