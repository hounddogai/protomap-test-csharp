namespace Acme.Workforce.Absence.Tests.LeaveRequest.Proposal;

using Acme.Workforce.Absence.LeaveRequest.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class RegionalLeaveRequestItemChecks
{
    public static void RegionalLeaveRequestItemStartsInCreated()
    {
        var item = RegionalLeaveRequestItem.NewRegionalLeaveRequestItem("code-739", "label-952");
        Check.Equal(item.Status, RegionalLeaveRequestItemStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalLeaveRequestItemMovesToPublished()
    {
        var item = RegionalLeaveRequestItem.NewRegionalLeaveRequestItem("code-739", "label-952");
        try
        {
            item.MoveTo(RegionalLeaveRequestItemStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalLeaveRequestItemStatus.Published, "status");
    }

    public static void RegionalLeaveRequestItemRejectsMovesFromRetired()
    {
        var item = RegionalLeaveRequestItem.NewRegionalLeaveRequestItem("code-739", "label-952");
        item.Status = RegionalLeaveRequestItemStatus.Retired;
        Check.Fails(() => item.MoveTo(RegionalLeaveRequestItemStatus.Created), "move");
    }

    public static void RunAll()
    {
        RegionalLeaveRequestItemStartsInCreated();
        RegionalLeaveRequestItemMovesToPublished();
        RegionalLeaveRequestItemRejectsMovesFromRetired();
    }
}
