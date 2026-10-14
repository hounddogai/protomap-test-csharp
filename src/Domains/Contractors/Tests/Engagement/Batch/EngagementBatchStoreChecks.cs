namespace Acme.Workforce.Contractors.Tests.Engagement.Batch;

using Acme.Workforce.Contractors.Engagement.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class EngagementBatchStoreChecks
{
    public static void MemoryEngagementBatchStoreFindsWhatItSaved()
    {
        var store = EngagementBatchStore.NewMemoryEngagementBatchStore();
        var item = EngagementBatch.NewEngagementBatch("code-368", "label-488");
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
        MemoryEngagementBatchStoreFindsWhatItSaved();
    }
}
