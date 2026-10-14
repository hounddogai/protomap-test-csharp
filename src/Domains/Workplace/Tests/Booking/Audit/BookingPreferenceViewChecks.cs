namespace Acme.Workforce.Workplace.Tests.Booking.Audit;

using Acme.Workforce.Workplace.Booking.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class BookingPreferenceViewChecks
{
    public static void BookingPreferenceViewShowsTheStatusInCapitals()
    {
        var item = BookingPreference.NewBookingPreference("code-246", "label-286");
        var view = BookingPreferenceView.ToBookingPreferenceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BookingPreferenceView.ToBookingPreferenceViews(new List<BookingPreference> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BookingPreferenceViewShowsTheStatusInCapitals();
    }
}
