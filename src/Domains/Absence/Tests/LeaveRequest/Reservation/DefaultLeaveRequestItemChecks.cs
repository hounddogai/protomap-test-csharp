namespace Acme.Workforce.Absence.Tests.LeaveRequest.Reservation;

using Acme.Workforce.Absence.LeaveRequest.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class DefaultLeaveRequestItemChecks
{
    public static void DefaultLeaveRequestItemStartsInOpen()
    {
        var item = DefaultLeaveRequestItem.NewDefaultLeaveRequestItem("code-401", "title-451");
        Check.Equal(item.Status, DefaultLeaveRequestItemStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultLeaveRequestItemMovesToResolved()
    {
        var item = DefaultLeaveRequestItem.NewDefaultLeaveRequestItem("code-401", "title-451");
        try
        {
            item.MoveTo(DefaultLeaveRequestItemStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultLeaveRequestItemStatus.Resolved, "status");
    }

    public static void DefaultLeaveRequestItemRejectsMovesFromArchived()
    {
        var item = DefaultLeaveRequestItem.NewDefaultLeaveRequestItem("code-401", "title-451");
        item.Status = DefaultLeaveRequestItemStatus.Archived;
        Check.Fails(() => item.MoveTo(DefaultLeaveRequestItemStatus.Open), "move");
    }

    public static void RunAll()
    {
        DefaultLeaveRequestItemStartsInOpen();
        DefaultLeaveRequestItemMovesToResolved();
        DefaultLeaveRequestItemRejectsMovesFromArchived();
    }
}
