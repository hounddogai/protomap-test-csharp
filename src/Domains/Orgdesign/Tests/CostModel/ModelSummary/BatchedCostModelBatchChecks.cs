namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelSummary;

using Acme.Workforce.Orgdesign.CostModel.ModelSummary;

/// <summary>Checks of model summary code, run by the project's runner.</summary>
public static class BatchedCostModelBatchChecks
{
    public static void BatchedCostModelBatchStartsInDraft()
    {
        var item = BatchedCostModelBatch.NewBatchedCostModelBatch("code-602", "title-675");
        Check.Equal(item.Status, BatchedCostModelBatchStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedCostModelBatchMovesToPending()
    {
        var item = BatchedCostModelBatch.NewBatchedCostModelBatch("code-602", "title-675");
        try
        {
            item.MoveTo(BatchedCostModelBatchStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedCostModelBatchStatus.Pending, "status");
    }

    public static void BatchedCostModelBatchRejectsMovesFromClosed()
    {
        var item = BatchedCostModelBatch.NewBatchedCostModelBatch("code-602", "title-675");
        item.Status = BatchedCostModelBatchStatus.Closed;
        Check.Fails(() => item.MoveTo(BatchedCostModelBatchStatus.Draft), "move");
    }

    public static void RunAll()
    {
        BatchedCostModelBatchStartsInDraft();
        BatchedCostModelBatchMovesToPending();
        BatchedCostModelBatchRejectsMovesFromClosed();
    }
}
