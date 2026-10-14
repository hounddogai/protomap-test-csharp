namespace Acme.Workforce.Absence.Tests.LeaveRequest.Draft;

using Acme.Workforce.Absence.LeaveRequest.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class ScheduledLeaveRequestSnapshotChecks
{
    public static void ScheduledLeaveRequestSnapshotStartsInIdle()
    {
        var item = ScheduledLeaveRequestSnapshot.NewScheduledLeaveRequestSnapshot("code-12", "name-996");
        Check.Equal(item.Status, ScheduledLeaveRequestSnapshotStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledLeaveRequestSnapshotMovesToWarming()
    {
        var item = ScheduledLeaveRequestSnapshot.NewScheduledLeaveRequestSnapshot("code-12", "name-996");
        try
        {
            item.MoveTo(ScheduledLeaveRequestSnapshotStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledLeaveRequestSnapshotStatus.Warming, "status");
    }

    public static void ScheduledLeaveRequestSnapshotRejectsMovesFromStopped()
    {
        var item = ScheduledLeaveRequestSnapshot.NewScheduledLeaveRequestSnapshot("code-12", "name-996");
        item.Status = ScheduledLeaveRequestSnapshotStatus.Stopped;
        Check.Fails(() => item.MoveTo(ScheduledLeaveRequestSnapshotStatus.Idle), "move");
    }

    public static void RunAll()
    {
        ScheduledLeaveRequestSnapshotStartsInIdle();
        ScheduledLeaveRequestSnapshotMovesToWarming();
        ScheduledLeaveRequestSnapshotRejectsMovesFromStopped();
    }
}
