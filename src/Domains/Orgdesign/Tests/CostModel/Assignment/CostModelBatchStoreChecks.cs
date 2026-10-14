namespace Acme.Workforce.Orgdesign.Tests.CostModel.Assignment;

using Acme.Workforce.Orgdesign.CostModel.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class CostModelBatchStoreChecks
{
    public static void MemoryCostModelBatchStoreFindsWhatItSaved()
    {
        var store = CostModelBatchStore.NewMemoryCostModelBatchStore();
        var item = CostModelBatch.NewCostModelBatch("code-697", "name-888");
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
        MemoryCostModelBatchStoreFindsWhatItSaved();
    }
}
