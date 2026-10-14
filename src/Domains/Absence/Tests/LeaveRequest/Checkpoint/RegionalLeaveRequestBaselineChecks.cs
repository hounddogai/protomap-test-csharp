namespace Acme.Workforce.Absence.Tests.LeaveRequest.Checkpoint;

using Acme.Workforce.Absence.LeaveRequest.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class RegionalLeaveRequestBaselineChecks
{
    public static void RegionalLeaveRequestBaselineStartsInIdle()
    {
        var item = RegionalLeaveRequestBaseline.NewRegionalLeaveRequestBaseline("code-765", "name-34");
        Check.Equal(item.Status, RegionalLeaveRequestBaselineStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalLeaveRequestBaselineMovesToWarming()
    {
        var item = RegionalLeaveRequestBaseline.NewRegionalLeaveRequestBaseline("code-765", "name-34");
        try
        {
            item.MoveTo(RegionalLeaveRequestBaselineStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalLeaveRequestBaselineStatus.Warming, "status");
    }

    public static void RegionalLeaveRequestBaselineRejectsMovesFromStopped()
    {
        var item = RegionalLeaveRequestBaseline.NewRegionalLeaveRequestBaseline("code-765", "name-34");
        item.Status = RegionalLeaveRequestBaselineStatus.Stopped;
        Check.Fails(() => item.MoveTo(RegionalLeaveRequestBaselineStatus.Idle), "move");
    }

    public static void RunAll()
    {
        RegionalLeaveRequestBaselineStartsInIdle();
        RegionalLeaveRequestBaselineMovesToWarming();
        RegionalLeaveRequestBaselineRejectsMovesFromStopped();
    }
}
