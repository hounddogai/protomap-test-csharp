namespace Acme.Workforce.Contractors.Tests.Engagement.EngagementSnapshot;

using Acme.Workforce.Contractors.Engagement.EngagementSnapshot;

/// <summary>Checks of engagement snapshot code, run by the project's runner.</summary>
public static class StrictEngagementSnapshotStoreChecks
{
    public static void MemoryStrictEngagementSnapshotStoreFindsWhatItSaved()
    {
        var store = StrictEngagementSnapshotStore.NewMemoryStrictEngagementSnapshotStore();
        var item = StrictEngagementSnapshot.NewStrictEngagementSnapshot("code-824", "title-67");
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
        MemoryStrictEngagementSnapshotStoreFindsWhatItSaved();
    }
}
