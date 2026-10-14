namespace Acme.Workforce.Workplace.Tests.Booking.Baseline;

using Acme.Workforce.Workplace.Booking.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class ScheduledBookingClaimChecks
{
    public static void ScheduledBookingClaimStartsInNew()
    {
        var item = ScheduledBookingClaim.NewScheduledBookingClaim("code-857", "title-716");
        Check.Equal(item.Status, ScheduledBookingClaimStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledBookingClaimMovesToQueued()
    {
        var item = ScheduledBookingClaim.NewScheduledBookingClaim("code-857", "title-716");
        try
        {
            item.MoveTo(ScheduledBookingClaimStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledBookingClaimStatus.Queued, "status");
    }

    public static void ScheduledBookingClaimRejectsMovesFromFailed()
    {
        var item = ScheduledBookingClaim.NewScheduledBookingClaim("code-857", "title-716");
        item.Status = ScheduledBookingClaimStatus.Failed;
        Check.Fails(() => item.MoveTo(ScheduledBookingClaimStatus.New), "move");
    }

    public static void RunAll()
    {
        ScheduledBookingClaimStartsInNew();
        ScheduledBookingClaimMovesToQueued();
        ScheduledBookingClaimRejectsMovesFromFailed();
    }
}
