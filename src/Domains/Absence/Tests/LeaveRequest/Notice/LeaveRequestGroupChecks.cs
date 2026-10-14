namespace Acme.Workforce.Absence.Tests.LeaveRequest.Notice;

using Acme.Workforce.Absence.LeaveRequest.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class LeaveRequestGroupChecks
{
    public static void LeaveRequestGroupStartsInCreated()
    {
        var item = LeaveRequestGroup.NewLeaveRequestGroup("code-31", "name-777");
        Check.Equal(item.Status, LeaveRequestGroupStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LeaveRequestGroupMovesToPublished()
    {
        var item = LeaveRequestGroup.NewLeaveRequestGroup("code-31", "name-777");
        try
        {
            item.MoveTo(LeaveRequestGroupStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LeaveRequestGroupStatus.Published, "status");
    }

    public static void LeaveRequestGroupRejectsMovesFromRetired()
    {
        var item = LeaveRequestGroup.NewLeaveRequestGroup("code-31", "name-777");
        item.Status = LeaveRequestGroupStatus.Retired;
        Check.Fails(() => item.MoveTo(LeaveRequestGroupStatus.Created), "move");
    }

    public static void RunAll()
    {
        LeaveRequestGroupStartsInCreated();
        LeaveRequestGroupMovesToPublished();
        LeaveRequestGroupRejectsMovesFromRetired();
    }
}
