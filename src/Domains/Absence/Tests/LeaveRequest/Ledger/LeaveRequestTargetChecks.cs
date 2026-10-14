namespace Acme.Workforce.Absence.Tests.LeaveRequest.Ledger;

using Acme.Workforce.Absence.LeaveRequest.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class LeaveRequestTargetChecks
{
    public static void LeaveRequestTargetStartsInPlanned()
    {
        var item = LeaveRequestTarget.NewLeaveRequestTarget("code-665", "label-769");
        Check.Equal(item.Status, LeaveRequestTargetStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LeaveRequestTargetMovesToCompleted()
    {
        var item = LeaveRequestTarget.NewLeaveRequestTarget("code-665", "label-769");
        try
        {
            item.MoveTo(LeaveRequestTargetStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LeaveRequestTargetStatus.Completed, "status");
    }

    public static void LeaveRequestTargetRejectsMovesFromCancelled()
    {
        var item = LeaveRequestTarget.NewLeaveRequestTarget("code-665", "label-769");
        item.Status = LeaveRequestTargetStatus.Cancelled;
        Check.Fails(() => item.MoveTo(LeaveRequestTargetStatus.Planned), "move");
    }

    public static void RunAll()
    {
        LeaveRequestTargetStartsInPlanned();
        LeaveRequestTargetMovesToCompleted();
        LeaveRequestTargetRejectsMovesFromCancelled();
    }
}
