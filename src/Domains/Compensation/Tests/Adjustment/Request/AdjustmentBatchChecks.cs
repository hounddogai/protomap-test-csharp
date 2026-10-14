namespace Acme.Workforce.Compensation.Tests.Adjustment.Request;

using Acme.Workforce.Compensation.Adjustment.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class AdjustmentBatchChecks
{
    public static void AdjustmentBatchStartsInPlanned()
    {
        var item = AdjustmentBatch.NewAdjustmentBatch("code-467", "label-64");
        Check.Equal(item.Status, AdjustmentBatchStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdjustmentBatchMovesToCompleted()
    {
        var item = AdjustmentBatch.NewAdjustmentBatch("code-467", "label-64");
        try
        {
            item.MoveTo(AdjustmentBatchStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdjustmentBatchStatus.Completed, "status");
    }

    public static void AdjustmentBatchRejectsMovesFromCancelled()
    {
        var item = AdjustmentBatch.NewAdjustmentBatch("code-467", "label-64");
        item.Status = AdjustmentBatchStatus.Cancelled;
        Check.Fails(() => item.MoveTo(AdjustmentBatchStatus.Planned), "move");
    }

    public static void RunAll()
    {
        AdjustmentBatchStartsInPlanned();
        AdjustmentBatchMovesToCompleted();
        AdjustmentBatchRejectsMovesFromCancelled();
    }
}
