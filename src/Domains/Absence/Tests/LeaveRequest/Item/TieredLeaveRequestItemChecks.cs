namespace Acme.Workforce.Absence.Tests.LeaveRequest.Item;

using Acme.Workforce.Absence.LeaveRequest.Item;

/// <summary>Checks of item code, run by the project's runner.</summary>
public static class TieredLeaveRequestItemChecks
{
    public static void TieredLeaveRequestItemStartsInRequested()
    {
        var item = TieredLeaveRequestItem.NewTieredLeaveRequestItem("code-580", "title-220");
        Check.Equal(item.Status, TieredLeaveRequestItemStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredLeaveRequestItemMovesToExpired()
    {
        var item = TieredLeaveRequestItem.NewTieredLeaveRequestItem("code-580", "title-220");
        try
        {
            item.MoveTo(TieredLeaveRequestItemStatus.Expired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredLeaveRequestItemStatus.Expired, "status");
    }

    public static void TieredLeaveRequestItemRejectsMovesFromExpired()
    {
        var item = TieredLeaveRequestItem.NewTieredLeaveRequestItem("code-580", "title-220");
        item.Status = TieredLeaveRequestItemStatus.Expired;
        Check.Fails(() => item.MoveTo(TieredLeaveRequestItemStatus.Requested), "move");
    }

    public static void RunAll()
    {
        TieredLeaveRequestItemStartsInRequested();
        TieredLeaveRequestItemMovesToExpired();
        TieredLeaveRequestItemRejectsMovesFromExpired();
    }
}
