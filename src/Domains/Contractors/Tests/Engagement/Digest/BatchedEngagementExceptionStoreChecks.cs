namespace Acme.Workforce.Contractors.Tests.Engagement.Digest;

using Acme.Workforce.Contractors.Engagement.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class BatchedEngagementExceptionStoreChecks
{
    public static void MemoryBatchedEngagementExceptionStoreFindsWhatItSaved()
    {
        var store = BatchedEngagementExceptionStore.NewMemoryBatchedEngagementExceptionStore();
        var item = BatchedEngagementException.NewBatchedEngagementException("code-208", "label-893");
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
        MemoryBatchedEngagementExceptionStoreFindsWhatItSaved();
    }
}
