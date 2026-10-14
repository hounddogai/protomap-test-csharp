namespace Acme.Workforce.Assets.Tests.AssetReturn.Budget;

using Acme.Workforce.Assets.AssetReturn.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class BatchedAssetReturnBudgetStoreChecks
{
    public static void MemoryBatchedAssetReturnBudgetStoreFindsWhatItSaved()
    {
        var store = BatchedAssetReturnBudgetStore.NewMemoryBatchedAssetReturnBudgetStore();
        var item = BatchedAssetReturnBudget.NewBatchedAssetReturnBudget("code-282", "title-337");
        store.Save(item);
        if (store.Find(item.Code) is not { } found)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(found.Code, item.Code, "code");
        Check.Equal(store.List().Count, 1, "count");
    }

    public static void RunAll()
    {
        MemoryBatchedAssetReturnBudgetStoreFindsWhatItSaved();
    }
}
