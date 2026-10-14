namespace Acme.Workforce.Workplace.Tests.Booking.Audit;

using Acme.Workforce.Workplace.Booking.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class BookingProfileViewChecks
{
    public static void BookingProfileViewShowsTheStatusInCapitals()
    {
        var item = BookingProfile.NewBookingProfile("code-249", "label-779");
        var view = BookingProfileView.ToBookingProfileView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BookingProfileView.ToBookingProfileViews(new List<BookingProfile> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BookingProfileViewShowsTheStatusInCapitals();
    }
}
