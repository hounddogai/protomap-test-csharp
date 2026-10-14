namespace Acme.Workforce.Absence.Tests.LeaveRequest.RequestBudget;

using Acme.Workforce.Absence.LeaveRequest.RequestBudget;

/// <summary>Checks of request budget code, run by the project's runner.</summary>
public static class LeaveRequestProfileChecks
{
    public static void LeaveRequestProfileStartsInPlanned()
    {
        var item = LeaveRequestProfile.NewLeaveRequestProfile("code-637", "label-396");
        Check.Equal(item.Status, LeaveRequestProfileStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LeaveRequestProfileMovesToCancelled()
    {
        var item = LeaveRequestProfile.NewLeaveRequestProfile("code-637", "label-396");
        try
        {
            item.MoveTo(LeaveRequestProfileStatus.Cancelled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LeaveRequestProfileStatus.Cancelled, "status");
    }

    public static void LeaveRequestProfileRejectsMovesFromCancelled()
    {
        var item = LeaveRequestProfile.NewLeaveRequestProfile("code-637", "label-396");
        item.Status = LeaveRequestProfileStatus.Cancelled;
        Check.Fails(() => item.MoveTo(LeaveRequestProfileStatus.Planned), "move");
    }

    public static void RunAll()
    {
        LeaveRequestProfileStartsInPlanned();
        LeaveRequestProfileMovesToCancelled();
        LeaveRequestProfileRejectsMovesFromCancelled();
    }
}
