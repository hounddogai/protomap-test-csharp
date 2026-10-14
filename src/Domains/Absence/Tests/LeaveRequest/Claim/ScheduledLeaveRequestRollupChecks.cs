namespace Acme.Workforce.Absence.Tests.LeaveRequest.Claim;

using Acme.Workforce.Absence.LeaveRequest.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class ScheduledLeaveRequestRollupChecks
{
    public static void ScheduledLeaveRequestRollupStartsInPlanned()
    {
        var item = ScheduledLeaveRequestRollup.NewScheduledLeaveRequestRollup("code-521", "label-500");
        Check.Equal(item.Status, ScheduledLeaveRequestRollupStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledLeaveRequestRollupMovesToScheduled()
    {
        var item = ScheduledLeaveRequestRollup.NewScheduledLeaveRequestRollup("code-521", "label-500");
        try
        {
            item.MoveTo(ScheduledLeaveRequestRollupStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledLeaveRequestRollupStatus.Scheduled, "status");
    }

    public static void ScheduledLeaveRequestRollupRejectsMovesFromCancelled()
    {
        var item = ScheduledLeaveRequestRollup.NewScheduledLeaveRequestRollup("code-521", "label-500");
        item.Status = ScheduledLeaveRequestRollupStatus.Cancelled;
        Check.Fails(() => item.MoveTo(ScheduledLeaveRequestRollupStatus.Planned), "move");
    }

    public static void RunAll()
    {
        ScheduledLeaveRequestRollupStartsInPlanned();
        ScheduledLeaveRequestRollupMovesToScheduled();
        ScheduledLeaveRequestRollupRejectsMovesFromCancelled();
    }
}
