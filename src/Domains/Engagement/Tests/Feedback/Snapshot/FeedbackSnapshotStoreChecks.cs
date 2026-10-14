namespace Acme.Workforce.Engagement.Tests.Feedback.Snapshot;

using Acme.Workforce.Engagement.Feedback.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class FeedbackSnapshotStoreChecks
{
    public static void MemoryFeedbackSnapshotStoreFindsWhatItSaved()
    {
        var store = FeedbackSnapshotStore.NewMemoryFeedbackSnapshotStore();
        var item = FeedbackSnapshot.NewFeedbackSnapshot("code-629", "label-43");
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
        MemoryFeedbackSnapshotStoreFindsWhatItSaved();
    }
}
