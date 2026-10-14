namespace Acme.Workforce.Absence.Tests.LeaveRequest.Draft;

using Acme.Workforce.Absence.LeaveRequest.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class LeaveRequestDraftChecks
{
    public static void LeaveRequestDraftStartsInPlanned()
    {
        var item = LeaveRequestDraft.NewLeaveRequestDraft("code-558", "title-175");
        Check.Equal(item.Status, LeaveRequestDraftStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LeaveRequestDraftMovesToInProgress()
    {
        var item = LeaveRequestDraft.NewLeaveRequestDraft("code-558", "title-175");
        try
        {
            item.MoveTo(LeaveRequestDraftStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LeaveRequestDraftStatus.InProgress, "status");
    }

    public static void LeaveRequestDraftRejectsMovesFromCancelled()
    {
        var item = LeaveRequestDraft.NewLeaveRequestDraft("code-558", "title-175");
        item.Status = LeaveRequestDraftStatus.Cancelled;
        Check.Fails(() => item.MoveTo(LeaveRequestDraftStatus.Planned), "move");
    }

    public static void RunAll()
    {
        LeaveRequestDraftStartsInPlanned();
        LeaveRequestDraftMovesToInProgress();
        LeaveRequestDraftRejectsMovesFromCancelled();
    }
}
