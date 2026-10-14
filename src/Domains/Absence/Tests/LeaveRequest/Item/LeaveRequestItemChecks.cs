namespace Acme.Workforce.Absence.Tests.LeaveRequest.Item;

using Acme.Workforce.Absence.LeaveRequest.Item;

/// <summary>Checks of item code, run by the project's runner.</summary>
public static class LeaveRequestItemChecks
{
    public static void LeaveRequestItemStartsInCreated()
    {
        var item = LeaveRequestItem.NewLeaveRequestItem("code-716", "title-327");
        Check.Equal(item.Status, LeaveRequestItemStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LeaveRequestItemMovesToValidated()
    {
        var item = LeaveRequestItem.NewLeaveRequestItem("code-716", "title-327");
        try
        {
            item.MoveTo(LeaveRequestItemStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LeaveRequestItemStatus.Validated, "status");
    }

    public static void LeaveRequestItemRejectsMovesFromRetired()
    {
        var item = LeaveRequestItem.NewLeaveRequestItem("code-716", "title-327");
        item.Status = LeaveRequestItemStatus.Retired;
        Check.Fails(() => item.MoveTo(LeaveRequestItemStatus.Created), "move");
    }

    public static void RunAll()
    {
        LeaveRequestItemStartsInCreated();
        LeaveRequestItemMovesToValidated();
        LeaveRequestItemRejectsMovesFromRetired();
    }
}
