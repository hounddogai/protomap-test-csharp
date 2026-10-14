namespace Acme.Workforce.Workplace.Tests.Booking.Digest;

using Acme.Workforce.Workplace.Booking.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class PendingBookingDigestViewChecks
{
    public static void PendingBookingDigestViewShowsTheStatusInCapitals()
    {
        var item = PendingBookingDigest.NewPendingBookingDigest("code-412", "label-174");
        var view = PendingBookingDigestView.ToPendingBookingDigestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PendingBookingDigestView.ToPendingBookingDigestViews(new List<PendingBookingDigest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PendingBookingDigestViewShowsTheStatusInCapitals();
    }
}
