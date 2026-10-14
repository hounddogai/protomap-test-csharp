namespace Acme.Workforce.Workplace.Tests.Booking.Digest;

using Acme.Workforce.Workplace.Booking.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class BookingNoticeViewChecks
{
    public static void BookingNoticeViewShowsTheStatusInCapitals()
    {
        var item = BookingNotice.NewBookingNotice("code-369", "title-778");
        var view = BookingNoticeView.ToBookingNoticeView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BookingNoticeView.ToBookingNoticeViews(new List<BookingNotice> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BookingNoticeViewShowsTheStatusInCapitals();
    }
}
