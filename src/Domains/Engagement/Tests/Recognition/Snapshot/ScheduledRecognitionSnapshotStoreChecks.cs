namespace Acme.Workforce.Engagement.Tests.Recognition.Snapshot;

using Acme.Workforce.Engagement.Recognition.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class ScheduledRecognitionSnapshotStoreChecks
{
    public static void MemoryScheduledRecognitionSnapshotStoreFindsWhatItSaved()
    {
        var store = ScheduledRecognitionSnapshotStore.NewMemoryScheduledRecognitionSnapshotStore();
        var item = ScheduledRecognitionSnapshot.NewScheduledRecognitionSnapshot("code-862", "title-413");
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
        MemoryScheduledRecognitionSnapshotStoreFindsWhatItSaved();
    }
}
