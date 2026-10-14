namespace Acme.Workforce.Engagement.Tests.Feedback.FeedbackProposal;

using Acme.Workforce.Engagement.Feedback.FeedbackProposal;

/// <summary>Checks of feedback proposal code, run by the project's runner.</summary>
public static class FeedbackEntryStoreChecks
{
    public static void MemoryFeedbackEntryStoreFindsWhatItSaved()
    {
        var store = FeedbackEntryStore.NewMemoryFeedbackEntryStore();
        var item = FeedbackEntry.NewFeedbackEntry("code-51", "label-215");
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
        MemoryFeedbackEntryStoreFindsWhatItSaved();
    }
}
