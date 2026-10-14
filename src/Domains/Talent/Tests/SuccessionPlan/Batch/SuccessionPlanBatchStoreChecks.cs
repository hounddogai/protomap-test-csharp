namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Batch;

using Acme.Workforce.Talent.SuccessionPlan.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class SuccessionPlanBatchStoreChecks
{
    public static void MemorySuccessionPlanBatchStoreFindsWhatItSaved()
    {
        var store = SuccessionPlanBatchStore.NewMemorySuccessionPlanBatchStore();
        var item = SuccessionPlanBatch.NewSuccessionPlanBatch("code-236", "title-560");
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
        MemorySuccessionPlanBatchStoreFindsWhatItSaved();
    }
}
