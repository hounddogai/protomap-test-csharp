namespace Acme.Workforce.Workplace.Tests.Booking.Plan;

using Acme.Workforce.Workplace.Booking.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class BookingMetricViewChecks
{
    public static void BookingMetricViewShowsTheStatusInCapitals()
    {
        var item = BookingMetric.NewBookingMetric("code-748", "name-865");
        var view = BookingMetricView.ToBookingMetricView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BookingMetricView.ToBookingMetricViews(new List<BookingMetric> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BookingMetricViewShowsTheStatusInCapitals();
    }
}
