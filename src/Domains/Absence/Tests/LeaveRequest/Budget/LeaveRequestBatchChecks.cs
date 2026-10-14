namespace Acme.Workforce.Absence.Tests.LeaveRequest.Budget;

using Acme.Workforce.Absence.LeaveRequest.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class LeaveRequestBatchChecks
{
    public static void LeaveRequestBatchStartsInDraft()
    {
        var item = LeaveRequestBatch.NewLeaveRequestBatch("code-117", "name-107");
        Check.Equal(item.Status, LeaveRequestBatchStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LeaveRequestBatchMovesToPending()
    {
        var item = LeaveRequestBatch.NewLeaveRequestBatch("code-117", "name-107");
        try
        {
            item.MoveTo(LeaveRequestBatchStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LeaveRequestBatchStatus.Pending, "status");
    }

    public static void LeaveRequestBatchRejectsMovesFromClosed()
    {
        var item = LeaveRequestBatch.NewLeaveRequestBatch("code-117", "name-107");
        item.Status = LeaveRequestBatchStatus.Closed;
        Check.Fails(() => item.MoveTo(LeaveRequestBatchStatus.Draft), "move");
    }

    public static void RunAll()
    {
        LeaveRequestBatchStartsInDraft();
        LeaveRequestBatchMovesToPending();
        LeaveRequestBatchRejectsMovesFromClosed();
    }
}
