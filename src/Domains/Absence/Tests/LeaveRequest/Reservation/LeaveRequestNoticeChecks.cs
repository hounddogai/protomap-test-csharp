namespace Acme.Workforce.Absence.Tests.LeaveRequest.Reservation;

using Acme.Workforce.Absence.LeaveRequest.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class LeaveRequestNoticeChecks
{
    public static void LeaveRequestNoticeStartsInProposed()
    {
        var item = LeaveRequestNotice.NewLeaveRequestNotice("code-764", "name-299");
        Check.Equal(item.Status, LeaveRequestNoticeStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LeaveRequestNoticeMovesToApproved()
    {
        var item = LeaveRequestNotice.NewLeaveRequestNotice("code-764", "name-299");
        try
        {
            item.MoveTo(LeaveRequestNoticeStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LeaveRequestNoticeStatus.Approved, "status");
    }

    public static void LeaveRequestNoticeRejectsMovesFromWithdrawn()
    {
        var item = LeaveRequestNotice.NewLeaveRequestNotice("code-764", "name-299");
        item.Status = LeaveRequestNoticeStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(LeaveRequestNoticeStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        LeaveRequestNoticeStartsInProposed();
        LeaveRequestNoticeMovesToApproved();
        LeaveRequestNoticeRejectsMovesFromWithdrawn();
    }
}
