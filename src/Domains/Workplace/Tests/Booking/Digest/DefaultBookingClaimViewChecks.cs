namespace Acme.Workforce.Workplace.Tests.Booking.Digest;

using Acme.Workforce.Workplace.Booking.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class DefaultBookingClaimViewChecks
{
    public static void DefaultBookingClaimViewShowsTheStatusInCapitals()
    {
        var item = DefaultBookingClaim.NewDefaultBookingClaim("code-592", "name-278");
        var view = DefaultBookingClaimView.ToDefaultBookingClaimView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DefaultBookingClaimView.ToDefaultBookingClaimViews(new List<DefaultBookingClaim> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DefaultBookingClaimViewShowsTheStatusInCapitals();
    }
}
