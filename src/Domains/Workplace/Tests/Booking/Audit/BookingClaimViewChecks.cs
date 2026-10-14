namespace Acme.Workforce.Workplace.Tests.Booking.Audit;

using Acme.Workforce.Workplace.Booking.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class BookingClaimViewChecks
{
    public static void BookingClaimViewShowsTheStatusInCapitals()
    {
        var item = BookingClaim.NewBookingClaim("code-604", "label-98");
        var view = BookingClaimView.ToBookingClaimView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BookingClaimView.ToBookingClaimViews(new List<BookingClaim> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BookingClaimViewShowsTheStatusInCapitals();
    }
}
