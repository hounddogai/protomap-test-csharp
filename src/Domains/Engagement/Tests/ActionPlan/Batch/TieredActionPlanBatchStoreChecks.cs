namespace Acme.Workforce.Engagement.Tests.ActionPlan.Batch;

using Acme.Workforce.Engagement.ActionPlan.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class TieredActionPlanBatchStoreChecks
{
    public static void MemoryTieredActionPlanBatchStoreFindsWhatItSaved()
    {
        var store = TieredActionPlanBatchStore.NewMemoryTieredActionPlanBatchStore();
        var item = TieredActionPlanBatch.NewTieredActionPlanBatch("code-836", "label-665");
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
        MemoryTieredActionPlanBatchStoreFindsWhatItSaved();
    }
}
