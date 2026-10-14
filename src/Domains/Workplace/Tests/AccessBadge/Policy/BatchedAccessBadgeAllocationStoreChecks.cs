namespace Acme.Workforce.Workplace.Tests.AccessBadge.Policy;

using Acme.Workforce.Workplace.AccessBadge.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class BatchedAccessBadgeAllocationStoreChecks
{
    public static void MemoryBatchedAccessBadgeAllocationStoreFindsWhatItSaved()
    {
        var store = BatchedAccessBadgeAllocationStore.NewMemoryBatchedAccessBadgeAllocationStore();
        var item = BatchedAccessBadgeAllocation.NewBatchedAccessBadgeAllocation("code-824", "title-597");
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
        MemoryBatchedAccessBadgeAllocationStoreFindsWhatItSaved();
    }
}
