namespace Acme.Workforce.Absence.Tests.LeaveRequest.Baseline;

using Acme.Workforce.Absence.LeaveRequest.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class LeaveRequestBaselineChecks
{
    public static void LeaveRequestBaselineStartsInNew()
    {
        var item = LeaveRequestBaseline.NewLeaveRequestBaseline("code-811", "title-360");
        Check.Equal(item.Status, LeaveRequestBaselineStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LeaveRequestBaselineMovesToQueued()
    {
        var item = LeaveRequestBaseline.NewLeaveRequestBaseline("code-811", "title-360");
        try
        {
            item.MoveTo(LeaveRequestBaselineStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LeaveRequestBaselineStatus.Queued, "status");
    }

    public static void LeaveRequestBaselineRejectsMovesFromFailed()
    {
        var item = LeaveRequestBaseline.NewLeaveRequestBaseline("code-811", "title-360");
        item.Status = LeaveRequestBaselineStatus.Failed;
        Check.Fails(() => item.MoveTo(LeaveRequestBaselineStatus.New), "move");
    }

    public static void RunAll()
    {
        LeaveRequestBaselineStartsInNew();
        LeaveRequestBaselineMovesToQueued();
        LeaveRequestBaselineRejectsMovesFromFailed();
    }
}
