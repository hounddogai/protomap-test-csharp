namespace Acme.Workforce.Absence.Tests.LeaveRequest.Item;

using Acme.Workforce.Absence.LeaveRequest.Item;

/// <summary>Checks of item code, run by the project's runner.</summary>
public static class LeaveRequestScoreChecks
{
    public static void LeaveRequestScoreStartsInPlanned()
    {
        var item = LeaveRequestScore.NewLeaveRequestScore("code-315", "title-787");
        Check.Equal(item.Status, LeaveRequestScoreStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LeaveRequestScoreMovesToScheduled()
    {
        var item = LeaveRequestScore.NewLeaveRequestScore("code-315", "title-787");
        try
        {
            item.MoveTo(LeaveRequestScoreStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LeaveRequestScoreStatus.Scheduled, "status");
    }

    public static void LeaveRequestScoreRejectsMovesFromCancelled()
    {
        var item = LeaveRequestScore.NewLeaveRequestScore("code-315", "title-787");
        item.Status = LeaveRequestScoreStatus.Cancelled;
        Check.Fails(() => item.MoveTo(LeaveRequestScoreStatus.Planned), "move");
    }

    public static void RunAll()
    {
        LeaveRequestScoreStartsInPlanned();
        LeaveRequestScoreMovesToScheduled();
        LeaveRequestScoreRejectsMovesFromCancelled();
    }
}
