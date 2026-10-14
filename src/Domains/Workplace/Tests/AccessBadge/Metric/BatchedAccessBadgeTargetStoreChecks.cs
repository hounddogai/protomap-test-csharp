namespace Acme.Workforce.Workplace.Tests.AccessBadge.Metric;

using Acme.Workforce.Workplace.AccessBadge.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class BatchedAccessBadgeTargetStoreChecks
{
    public static void MemoryBatchedAccessBadgeTargetStoreFindsWhatItSaved()
    {
        var store = BatchedAccessBadgeTargetStore.NewMemoryBatchedAccessBadgeTargetStore();
        var item = BatchedAccessBadgeTarget.NewBatchedAccessBadgeTarget("code-10", "title-996");
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
        MemoryBatchedAccessBadgeTargetStoreFindsWhatItSaved();
    }
}
