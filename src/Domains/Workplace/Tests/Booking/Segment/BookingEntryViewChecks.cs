namespace Acme.Workforce.Workplace.Tests.Booking.Segment;

using Acme.Workforce.Workplace.Booking.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class BookingEntryViewChecks
{
    public static void BookingEntryViewShowsTheStatusInCapitals()
    {
        var item = BookingEntry.NewBookingEntry("code-623", "title-581");
        var view = BookingEntryView.ToBookingEntryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BookingEntryView.ToBookingEntryViews(new List<BookingEntry> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BookingEntryViewShowsTheStatusInCapitals();
    }
}
