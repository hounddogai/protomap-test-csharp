namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelSummary;

using Acme.Workforce.Orgdesign.CostModel.ModelSummary;

/// <summary>Checks of model summary code, run by the project's runner.</summary>
public static class BatchedCostModelBatchStoreChecks
{
    public static void MemoryBatchedCostModelBatchStoreFindsWhatItSaved()
    {
        var store = BatchedCostModelBatchStore.NewMemoryBatchedCostModelBatchStore();
        var item = BatchedCostModelBatch.NewBatchedCostModelBatch("code-602", "title-675");
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
        MemoryBatchedCostModelBatchStoreFindsWhatItSaved();
    }
}
