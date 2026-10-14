namespace Acme.Workforce.Absence.Tests.LeaveRequest.RequestClaim;

using Acme.Workforce.Absence.LeaveRequest.RequestClaim;

/// <summary>Checks of request claim code, run by the project's runner.</summary>
public static class RollingLeaveRequestBatchChecks
{
    public static void RollingLeaveRequestBatchStartsInOpen()
    {
        var item = RollingLeaveRequestBatch.NewRollingLeaveRequestBatch("code-733", "label-849");
        Check.Equal(item.Status, RollingLeaveRequestBatchStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingLeaveRequestBatchMovesToResolved()
    {
        var item = RollingLeaveRequestBatch.NewRollingLeaveRequestBatch("code-733", "label-849");
        try
        {
            item.MoveTo(RollingLeaveRequestBatchStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingLeaveRequestBatchStatus.Resolved, "status");
    }

    public static void RollingLeaveRequestBatchRejectsMovesFromArchived()
    {
        var item = RollingLeaveRequestBatch.NewRollingLeaveRequestBatch("code-733", "label-849");
        item.Status = RollingLeaveRequestBatchStatus.Archived;
        Check.Fails(() => item.MoveTo(RollingLeaveRequestBatchStatus.Open), "move");
    }

    public static void RunAll()
    {
        RollingLeaveRequestBatchStartsInOpen();
        RollingLeaveRequestBatchMovesToResolved();
        RollingLeaveRequestBatchRejectsMovesFromArchived();
    }
}
