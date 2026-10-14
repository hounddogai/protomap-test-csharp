namespace Acme.Workforce.Workplace.Tests.Booking.Group;

using Acme.Workforce.Workplace.Booking.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class BookingManifestViewChecks
{
    public static void BookingManifestViewShowsTheStatusInCapitals()
    {
        var item = BookingManifest.NewBookingManifest("code-154", "name-5");
        var view = BookingManifestView.ToBookingManifestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BookingManifestView.ToBookingManifestViews(new List<BookingManifest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BookingManifestViewShowsTheStatusInCapitals();
    }
}
