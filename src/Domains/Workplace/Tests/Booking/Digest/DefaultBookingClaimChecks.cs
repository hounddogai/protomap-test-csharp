namespace Acme.Workforce.Workplace.Tests.Booking.Digest;

using Acme.Workforce.Workplace.Booking.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class DefaultBookingClaimChecks
{
    public static void DefaultBookingClaimStartsInNew()
    {
        var item = DefaultBookingClaim.NewDefaultBookingClaim("code-592", "name-278");
        Check.Equal(item.Status, DefaultBookingClaimStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultBookingClaimMovesToRunning()
    {
        var item = DefaultBookingClaim.NewDefaultBookingClaim("code-592", "name-278");
        try
        {
            item.MoveTo(DefaultBookingClaimStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultBookingClaimStatus.Running, "status");
    }

    public static void DefaultBookingClaimRejectsMovesFromFailed()
    {
        var item = DefaultBookingClaim.NewDefaultBookingClaim("code-592", "name-278");
        item.Status = DefaultBookingClaimStatus.Failed;
        Check.Fails(() => item.MoveTo(DefaultBookingClaimStatus.New), "move");
    }

    public static void RunAll()
    {
        DefaultBookingClaimStartsInNew();
        DefaultBookingClaimMovesToRunning();
        DefaultBookingClaimRejectsMovesFromFailed();
    }
}
