namespace Acme.Workforce.Absence.Tests.LeaveRequest.Batch;

using Acme.Workforce.Absence.LeaveRequest.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class PendingLeaveRequestBatchChecks
{
    public static void PendingLeaveRequestBatchStartsInProposed()
    {
        var item = PendingLeaveRequestBatch.NewPendingLeaveRequestBatch("code-711", "title-999");
        Check.Equal(item.Status, PendingLeaveRequestBatchStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingLeaveRequestBatchMovesToApproved()
    {
        var item = PendingLeaveRequestBatch.NewPendingLeaveRequestBatch("code-711", "title-999");
        try
        {
            item.MoveTo(PendingLeaveRequestBatchStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingLeaveRequestBatchStatus.Approved, "status");
    }

    public static void PendingLeaveRequestBatchRejectsMovesFromWithdrawn()
    {
        var item = PendingLeaveRequestBatch.NewPendingLeaveRequestBatch("code-711", "title-999");
        item.Status = PendingLeaveRequestBatchStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(PendingLeaveRequestBatchStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        PendingLeaveRequestBatchStartsInProposed();
        PendingLeaveRequestBatchMovesToApproved();
        PendingLeaveRequestBatchRejectsMovesFromWithdrawn();
    }
}
