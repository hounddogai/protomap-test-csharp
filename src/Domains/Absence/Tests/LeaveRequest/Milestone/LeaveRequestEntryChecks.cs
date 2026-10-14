namespace Acme.Workforce.Absence.Tests.LeaveRequest.Milestone;

using Acme.Workforce.Absence.LeaveRequest.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class LeaveRequestEntryChecks
{
    public static void LeaveRequestEntryStartsInIdle()
    {
        var item = LeaveRequestEntry.NewLeaveRequestEntry("code-903", "name-696");
        Check.Equal(item.Status, LeaveRequestEntryStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LeaveRequestEntryMovesToWarming()
    {
        var item = LeaveRequestEntry.NewLeaveRequestEntry("code-903", "name-696");
        try
        {
            item.MoveTo(LeaveRequestEntryStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LeaveRequestEntryStatus.Warming, "status");
    }

    public static void LeaveRequestEntryRejectsMovesFromStopped()
    {
        var item = LeaveRequestEntry.NewLeaveRequestEntry("code-903", "name-696");
        item.Status = LeaveRequestEntryStatus.Stopped;
        Check.Fails(() => item.MoveTo(LeaveRequestEntryStatus.Idle), "move");
    }

    public static void RunAll()
    {
        LeaveRequestEntryStartsInIdle();
        LeaveRequestEntryMovesToWarming();
        LeaveRequestEntryRejectsMovesFromStopped();
    }
}
