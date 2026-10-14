namespace Acme.Workforce.Workplace.Tests.Booking.Baseline;

using Acme.Workforce.Workplace.Booking.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class ScheduledBookingClaimViewChecks
{
    public static void ScheduledBookingClaimViewShowsTheStatusInCapitals()
    {
        var item = ScheduledBookingClaim.NewScheduledBookingClaim("code-857", "title-716");
        var view = ScheduledBookingClaimView.ToScheduledBookingClaimView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledBookingClaimView.ToScheduledBookingClaimViews(new List<ScheduledBookingClaim> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledBookingClaimViewShowsTheStatusInCapitals();
    }
}
