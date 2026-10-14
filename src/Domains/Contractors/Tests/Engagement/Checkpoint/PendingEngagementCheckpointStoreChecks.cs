namespace Acme.Workforce.Contractors.Tests.Engagement.Checkpoint;

using Acme.Workforce.Contractors.Engagement.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class PendingEngagementCheckpointStoreChecks
{
    public static void MemoryPendingEngagementCheckpointStoreFindsWhatItSaved()
    {
        var store = PendingEngagementCheckpointStore.NewMemoryPendingEngagementCheckpointStore();
        var item = PendingEngagementCheckpoint.NewPendingEngagementCheckpoint("code-302", "title-127");
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
        MemoryPendingEngagementCheckpointStoreFindsWhatItSaved();
    }
}
