namespace Acme.Workforce.Absence.Tests.LeaveRequest.Notice;

using Acme.Workforce.Absence.LeaveRequest.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class ScheduledLeaveRequestOverrideChecks
{
    public static void ScheduledLeaveRequestOverrideStartsInProposed()
    {
        var item = ScheduledLeaveRequestOverride.NewScheduledLeaveRequestOverride("code-968", "name-379");
        Check.Equal(item.Status, ScheduledLeaveRequestOverrideStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledLeaveRequestOverrideMovesToRejected()
    {
        var item = ScheduledLeaveRequestOverride.NewScheduledLeaveRequestOverride("code-968", "name-379");
        try
        {
            item.MoveTo(ScheduledLeaveRequestOverrideStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledLeaveRequestOverrideStatus.Rejected, "status");
    }

    public static void ScheduledLeaveRequestOverrideRejectsMovesFromWithdrawn()
    {
        var item = ScheduledLeaveRequestOverride.NewScheduledLeaveRequestOverride("code-968", "name-379");
        item.Status = ScheduledLeaveRequestOverrideStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(ScheduledLeaveRequestOverrideStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        ScheduledLeaveRequestOverrideStartsInProposed();
        ScheduledLeaveRequestOverrideMovesToRejected();
        ScheduledLeaveRequestOverrideRejectsMovesFromWithdrawn();
    }
}
