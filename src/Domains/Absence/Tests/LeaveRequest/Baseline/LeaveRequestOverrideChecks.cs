namespace Acme.Workforce.Absence.Tests.LeaveRequest.Baseline;

using Acme.Workforce.Absence.LeaveRequest.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class LeaveRequestOverrideChecks
{
    public static void LeaveRequestOverrideStartsInOpen()
    {
        var item = LeaveRequestOverride.NewLeaveRequestOverride("code-590", "label-209");
        Check.Equal(item.Status, LeaveRequestOverrideStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LeaveRequestOverrideMovesToResolved()
    {
        var item = LeaveRequestOverride.NewLeaveRequestOverride("code-590", "label-209");
        try
        {
            item.MoveTo(LeaveRequestOverrideStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LeaveRequestOverrideStatus.Resolved, "status");
    }

    public static void LeaveRequestOverrideRejectsMovesFromArchived()
    {
        var item = LeaveRequestOverride.NewLeaveRequestOverride("code-590", "label-209");
        item.Status = LeaveRequestOverrideStatus.Archived;
        Check.Fails(() => item.MoveTo(LeaveRequestOverrideStatus.Open), "move");
    }

    public static void RunAll()
    {
        LeaveRequestOverrideStartsInOpen();
        LeaveRequestOverrideMovesToResolved();
        LeaveRequestOverrideRejectsMovesFromArchived();
    }
}
