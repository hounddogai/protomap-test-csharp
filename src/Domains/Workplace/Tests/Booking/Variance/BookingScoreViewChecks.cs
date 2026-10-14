namespace Acme.Workforce.Workplace.Tests.Booking.Variance;

using Acme.Workforce.Workplace.Booking.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class BookingScoreViewChecks
{
    public static void BookingScoreViewShowsTheStatusInCapitals()
    {
        var item = BookingScore.NewBookingScore("code-413", "label-301");
        var view = BookingScoreView.ToBookingScoreView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BookingScoreView.ToBookingScoreViews(new List<BookingScore> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BookingScoreViewShowsTheStatusInCapitals();
    }
}
