namespace Acme.Workforce.Workplace.Tests.Booking.Audit;

using Acme.Workforce.Workplace.Booking.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class BookingClaimChecks
{
    public static void BookingClaimStartsInDraft()
    {
        var item = BookingClaim.NewBookingClaim("code-604", "label-98");
        Check.Equal(item.Status, BookingClaimStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BookingClaimMovesToActive()
    {
        var item = BookingClaim.NewBookingClaim("code-604", "label-98");
        try
        {
            item.MoveTo(BookingClaimStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BookingClaimStatus.Active, "status");
    }

    public static void BookingClaimRejectsMovesFromClosed()
    {
        var item = BookingClaim.NewBookingClaim("code-604", "label-98");
        item.Status = BookingClaimStatus.Closed;
        Check.Fails(() => item.MoveTo(BookingClaimStatus.Draft), "move");
    }

    public static void RunAll()
    {
        BookingClaimStartsInDraft();
        BookingClaimMovesToActive();
        BookingClaimRejectsMovesFromClosed();
    }
}
