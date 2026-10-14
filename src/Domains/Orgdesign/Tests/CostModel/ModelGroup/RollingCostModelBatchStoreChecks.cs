namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelGroup;

using Acme.Workforce.Orgdesign.CostModel.ModelGroup;

/// <summary>Checks of model group code, run by the project's runner.</summary>
public static class RollingCostModelBatchStoreChecks
{
    public static void MemoryRollingCostModelBatchStoreFindsWhatItSaved()
    {
        var store = RollingCostModelBatchStore.NewMemoryRollingCostModelBatchStore();
        var item = RollingCostModelBatch.NewRollingCostModelBatch("code-378", "label-206");
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
        MemoryRollingCostModelBatchStoreFindsWhatItSaved();
    }
}
