namespace Acme.Workforce.Workplace.Tests.Booking.Group;

using Acme.Workforce.Workplace.Booking.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class CappedBookingTargetChecks
{
    public static void CappedBookingTargetStartsInProposed()
    {
        var item = CappedBookingTarget.NewCappedBookingTarget("code-368", "name-196");
        Check.Equal(item.Status, CappedBookingTargetStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedBookingTargetMovesToRejected()
    {
        var item = CappedBookingTarget.NewCappedBookingTarget("code-368", "name-196");
        try
        {
            item.MoveTo(CappedBookingTargetStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedBookingTargetStatus.Rejected, "status");
    }

    public static void CappedBookingTargetRejectsMovesFromWithdrawn()
    {
        var item = CappedBookingTarget.NewCappedBookingTarget("code-368", "name-196");
        item.Status = CappedBookingTargetStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(CappedBookingTargetStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        CappedBookingTargetStartsInProposed();
        CappedBookingTargetMovesToRejected();
        CappedBookingTargetRejectsMovesFromWithdrawn();
    }
}
