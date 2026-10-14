namespace Acme.Workforce.Absence.Tests.LeaveRequest.Notice;

using Acme.Workforce.Absence.LeaveRequest.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class CappedLeaveRequestBaselineChecks
{
    public static void CappedLeaveRequestBaselineStartsInPlanned()
    {
        var item = CappedLeaveRequestBaseline.NewCappedLeaveRequestBaseline("code-560", "title-383");
        Check.Equal(item.Status, CappedLeaveRequestBaselineStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedLeaveRequestBaselineMovesToScheduled()
    {
        var item = CappedLeaveRequestBaseline.NewCappedLeaveRequestBaseline("code-560", "title-383");
        try
        {
            item.MoveTo(CappedLeaveRequestBaselineStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedLeaveRequestBaselineStatus.Scheduled, "status");
    }

    public static void CappedLeaveRequestBaselineRejectsMovesFromCancelled()
    {
        var item = CappedLeaveRequestBaseline.NewCappedLeaveRequestBaseline("code-560", "title-383");
        item.Status = CappedLeaveRequestBaselineStatus.Cancelled;
        Check.Fails(() => item.MoveTo(CappedLeaveRequestBaselineStatus.Planned), "move");
    }

    public static void RunAll()
    {
        CappedLeaveRequestBaselineStartsInPlanned();
        CappedLeaveRequestBaselineMovesToScheduled();
        CappedLeaveRequestBaselineRejectsMovesFromCancelled();
    }
}
