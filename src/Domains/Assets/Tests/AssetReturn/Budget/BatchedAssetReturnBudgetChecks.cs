namespace Acme.Workforce.Assets.Tests.AssetReturn.Budget;

using Acme.Workforce.Assets.AssetReturn.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class BatchedAssetReturnBudgetChecks
{
    public static void BatchedAssetReturnBudgetStartsInOpen()
    {
        var item = BatchedAssetReturnBudget.NewBatchedAssetReturnBudget("code-282", "title-337");
        Check.Equal(item.Status, BatchedAssetReturnBudgetStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedAssetReturnBudgetMovesToReopened()
    {
        var item = BatchedAssetReturnBudget.NewBatchedAssetReturnBudget("code-282", "title-337");
        try
        {
            item.MoveTo(BatchedAssetReturnBudgetStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedAssetReturnBudgetStatus.Reopened, "status");
    }

    public static void BatchedAssetReturnBudgetRejectsMovesFromArchived()
    {
        var item = BatchedAssetReturnBudget.NewBatchedAssetReturnBudget("code-282", "title-337");
        item.Status = BatchedAssetReturnBudgetStatus.Archived;
        Check.Fails(() => item.MoveTo(BatchedAssetReturnBudgetStatus.Open), "move");
    }

    public static void RunAll()
    {
        BatchedAssetReturnBudgetStartsInOpen();
        BatchedAssetReturnBudgetMovesToReopened();
        BatchedAssetReturnBudgetRejectsMovesFromArchived();
    }
}
