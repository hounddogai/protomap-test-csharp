namespace Acme.Workforce.Engagement.Tests.Feedback.Checkpoint;

using Acme.Workforce.Engagement.Feedback.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class FeedbackCheckpointStoreChecks
{
    public static void MemoryFeedbackCheckpointStoreFindsWhatItSaved()
    {
        var store = FeedbackCheckpointStore.NewMemoryFeedbackCheckpointStore();
        var item = FeedbackCheckpoint.NewFeedbackCheckpoint("code-610", "name-977");
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
        MemoryFeedbackCheckpointStoreFindsWhatItSaved();
    }
}
