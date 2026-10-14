namespace Acme.Workforce.Engagement.Tests.Feedback.Summary;

using Acme.Workforce.Engagement.Feedback.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class FeedbackTargetStoreChecks
{
    public static void MemoryFeedbackTargetStoreFindsWhatItSaved()
    {
        var store = FeedbackTargetStore.NewMemoryFeedbackTargetStore();
        var item = FeedbackTarget.NewFeedbackTarget("code-839", "title-876");
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
        MemoryFeedbackTargetStoreFindsWhatItSaved();
    }
}
