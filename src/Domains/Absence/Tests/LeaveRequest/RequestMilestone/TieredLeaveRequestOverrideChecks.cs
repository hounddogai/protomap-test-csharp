namespace Acme.Workforce.Absence.Tests.LeaveRequest.RequestMilestone;

using Acme.Workforce.Absence.LeaveRequest.RequestMilestone;

/// <summary>Checks of request milestone code, run by the project's runner.</summary>
public static class TieredLeaveRequestOverrideChecks
{
    public static void TieredLeaveRequestOverrideStartsInProposed()
    {
        var item = TieredLeaveRequestOverride.NewTieredLeaveRequestOverride("code-1", "title-262");
        Check.Equal(item.Status, TieredLeaveRequestOverrideStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredLeaveRequestOverrideMovesToRejected()
    {
        var item = TieredLeaveRequestOverride.NewTieredLeaveRequestOverride("code-1", "title-262");
        try
        {
            item.MoveTo(TieredLeaveRequestOverrideStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredLeaveRequestOverrideStatus.Rejected, "status");
    }

    public static void TieredLeaveRequestOverrideRejectsMovesFromWithdrawn()
    {
        var item = TieredLeaveRequestOverride.NewTieredLeaveRequestOverride("code-1", "title-262");
        item.Status = TieredLeaveRequestOverrideStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(TieredLeaveRequestOverrideStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        TieredLeaveRequestOverrideStartsInProposed();
        TieredLeaveRequestOverrideMovesToRejected();
        TieredLeaveRequestOverrideRejectsMovesFromWithdrawn();
    }
}
