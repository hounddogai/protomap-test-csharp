namespace Acme.Workforce.Workplace.Tests.Booking.BookingAudit;

using Acme.Workforce.Workplace.Booking.BookingAudit;

/// <summary>Checks of booking audit code, run by the project's runner.</summary>
public static class BookingSnapshotViewChecks
{
    public static void BookingSnapshotViewShowsTheStatusInCapitals()
    {
        var item = BookingSnapshot.NewBookingSnapshot("code-766", "name-996");
        var view = BookingSnapshotView.ToBookingSnapshotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BookingSnapshotView.ToBookingSnapshotViews(new List<BookingSnapshot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BookingSnapshotViewShowsTheStatusInCapitals();
    }
}
