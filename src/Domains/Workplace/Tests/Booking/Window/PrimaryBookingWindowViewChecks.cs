namespace Acme.Workforce.Workplace.Tests.Booking.Window;

using Acme.Workforce.Workplace.Booking.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class PrimaryBookingWindowViewChecks
{
    public static void PrimaryBookingWindowViewShowsTheStatusInCapitals()
    {
        var item = PrimaryBookingWindow.NewPrimaryBookingWindow("code-997", "name-788");
        var view = PrimaryBookingWindowView.ToPrimaryBookingWindowView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PrimaryBookingWindowView.ToPrimaryBookingWindowViews(new List<PrimaryBookingWindow> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PrimaryBookingWindowViewShowsTheStatusInCapitals();
    }
}
