namespace Acme.Workforce.Assets.Tests.Refresh.OverrideItem;

using Acme.Workforce.Assets.Refresh.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class BatchedRefreshAssignmentStoreChecks
{
    public static void MemoryBatchedRefreshAssignmentStoreFindsWhatItSaved()
    {
        var store = BatchedRefreshAssignmentStore.NewMemoryBatchedRefreshAssignmentStore();
        var item = BatchedRefreshAssignment.NewBatchedRefreshAssignment("code-903", "name-999");
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
        MemoryBatchedRefreshAssignmentStoreFindsWhatItSaved();
    }
}
