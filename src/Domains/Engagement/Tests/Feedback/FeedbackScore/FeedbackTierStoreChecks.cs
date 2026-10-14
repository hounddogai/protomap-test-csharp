namespace Acme.Workforce.Engagement.Tests.Feedback.FeedbackScore;

using Acme.Workforce.Engagement.Feedback.FeedbackScore;

/// <summary>Checks of feedback score code, run by the project's runner.</summary>
public static class FeedbackTierStoreChecks
{
    public static void MemoryFeedbackTierStoreFindsWhatItSaved()
    {
        var store = FeedbackTierStore.NewMemoryFeedbackTierStore();
        var item = FeedbackTier.NewFeedbackTier("code-421", "title-885");
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
        MemoryFeedbackTierStoreFindsWhatItSaved();
    }
}
