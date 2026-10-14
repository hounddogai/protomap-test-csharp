namespace Acme.Workforce.Workplace.Tests.Booking.Assignment;

using Acme.Workforce.Workplace.Booking.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class BookingForecastViewChecks
{
    public static void BookingForecastViewShowsTheStatusInCapitals()
    {
        var item = BookingForecast.NewBookingForecast("code-62", "title-438");
        var view = BookingForecastView.ToBookingForecastView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BookingForecastView.ToBookingForecastViews(new List<BookingForecast> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BookingForecastViewShowsTheStatusInCapitals();
    }
}
