namespace Acme.Workforce.Workplace.Tests.Booking.BookingReview;

using Acme.Workforce.Workplace.Booking.BookingReview;

/// <summary>Checks of booking review code, run by the project's runner.</summary>
public static class PrimaryBookingTierChecks
{
    public static void PrimaryBookingTierStartsInDraft()
    {
        var item = PrimaryBookingTier.NewPrimaryBookingTier("code-961", "title-625");
        Check.Equal(item.Status, PrimaryBookingTierStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryBookingTierMovesToPending()
    {
        var item = PrimaryBookingTier.NewPrimaryBookingTier("code-961", "title-625");
        try
        {
            item.MoveTo(PrimaryBookingTierStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryBookingTierStatus.Pending, "status");
    }

    public static void PrimaryBookingTierRejectsMovesFromClosed()
    {
        var item = PrimaryBookingTier.NewPrimaryBookingTier("code-961", "title-625");
        item.Status = PrimaryBookingTierStatus.Closed;
        Check.Fails(() => item.MoveTo(PrimaryBookingTierStatus.Draft), "move");
    }

    public static void RunAll()
    {
        PrimaryBookingTierStartsInDraft();
        PrimaryBookingTierMovesToPending();
        PrimaryBookingTierRejectsMovesFromClosed();
    }
}
