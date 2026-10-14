namespace Acme.Workforce.Compensation.Tests.Adjustment.Request;

using Acme.Workforce.Compensation.Adjustment.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class BatchedAdjustmentRequestChecks
{
    public static void BatchedAdjustmentRequestStartsInPlanned()
    {
        var item = BatchedAdjustmentRequest.NewBatchedAdjustmentRequest("code-109", "title-32");
        Check.Equal(item.Status, BatchedAdjustmentRequestStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedAdjustmentRequestMovesToCompleted()
    {
        var item = BatchedAdjustmentRequest.NewBatchedAdjustmentRequest("code-109", "title-32");
        try
        {
            item.MoveTo(BatchedAdjustmentRequestStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedAdjustmentRequestStatus.Completed, "status");
    }

    public static void BatchedAdjustmentRequestRejectsMovesFromCancelled()
    {
        var item = BatchedAdjustmentRequest.NewBatchedAdjustmentRequest("code-109", "title-32");
        item.Status = BatchedAdjustmentRequestStatus.Cancelled;
        Check.Fails(() => item.MoveTo(BatchedAdjustmentRequestStatus.Planned), "move");
    }

    public static void RunAll()
    {
        BatchedAdjustmentRequestStartsInPlanned();
        BatchedAdjustmentRequestMovesToCompleted();
        BatchedAdjustmentRequestRejectsMovesFromCancelled();
    }
}
