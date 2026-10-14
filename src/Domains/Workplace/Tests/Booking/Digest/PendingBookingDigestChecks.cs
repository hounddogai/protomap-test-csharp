namespace Acme.Workforce.Workplace.Tests.Booking.Digest;

using Acme.Workforce.Workplace.Booking.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class PendingBookingDigestChecks
{
    public static void PendingBookingDigestStartsInPlanned()
    {
        var item = PendingBookingDigest.NewPendingBookingDigest("code-412", "label-174");
        Check.Equal(item.Status, PendingBookingDigestStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingBookingDigestMovesToCancelled()
    {
        var item = PendingBookingDigest.NewPendingBookingDigest("code-412", "label-174");
        try
        {
            item.MoveTo(PendingBookingDigestStatus.Cancelled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingBookingDigestStatus.Cancelled, "status");
    }

    public static void PendingBookingDigestRejectsMovesFromCancelled()
    {
        var item = PendingBookingDigest.NewPendingBookingDigest("code-412", "label-174");
        item.Status = PendingBookingDigestStatus.Cancelled;
        Check.Fails(() => item.MoveTo(PendingBookingDigestStatus.Planned), "move");
    }

    public static void RunAll()
    {
        PendingBookingDigestStartsInPlanned();
        PendingBookingDigestMovesToCancelled();
        PendingBookingDigestRejectsMovesFromCancelled();
    }
}
