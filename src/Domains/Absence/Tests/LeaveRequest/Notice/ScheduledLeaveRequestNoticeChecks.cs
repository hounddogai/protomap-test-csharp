namespace Acme.Workforce.Absence.Tests.LeaveRequest.Notice;

using Acme.Workforce.Absence.LeaveRequest.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class ScheduledLeaveRequestNoticeChecks
{
    public static void ScheduledLeaveRequestNoticeStartsInDraft()
    {
        var item = ScheduledLeaveRequestNotice.NewScheduledLeaveRequestNotice("code-240", "name-628");
        Check.Equal(item.Status, ScheduledLeaveRequestNoticeStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledLeaveRequestNoticeMovesToPending()
    {
        var item = ScheduledLeaveRequestNotice.NewScheduledLeaveRequestNotice("code-240", "name-628");
        try
        {
            item.MoveTo(ScheduledLeaveRequestNoticeStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledLeaveRequestNoticeStatus.Pending, "status");
    }

    public static void ScheduledLeaveRequestNoticeRejectsMovesFromClosed()
    {
        var item = ScheduledLeaveRequestNotice.NewScheduledLeaveRequestNotice("code-240", "name-628");
        item.Status = ScheduledLeaveRequestNoticeStatus.Closed;
        Check.Fails(() => item.MoveTo(ScheduledLeaveRequestNoticeStatus.Draft), "move");
    }

    public static void RunAll()
    {
        ScheduledLeaveRequestNoticeStartsInDraft();
        ScheduledLeaveRequestNoticeMovesToPending();
        ScheduledLeaveRequestNoticeRejectsMovesFromClosed();
    }
}
