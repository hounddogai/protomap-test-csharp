namespace Acme.Workforce.Contractors.Tests.Engagement.Rollup;

using Acme.Workforce.Contractors.Engagement.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class BatchedEngagementVarianceStoreChecks
{
    public static void MemoryBatchedEngagementVarianceStoreFindsWhatItSaved()
    {
        var store = BatchedEngagementVarianceStore.NewMemoryBatchedEngagementVarianceStore();
        var item = BatchedEngagementVariance.NewBatchedEngagementVariance("code-615", "label-636");
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
        MemoryBatchedEngagementVarianceStoreFindsWhatItSaved();
    }
}
