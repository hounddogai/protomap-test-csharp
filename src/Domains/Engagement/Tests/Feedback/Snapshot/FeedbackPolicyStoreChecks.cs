namespace Acme.Workforce.Engagement.Tests.Feedback.Snapshot;

using Acme.Workforce.Engagement.Feedback.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class FeedbackPolicyStoreChecks
{
    public static void MemoryFeedbackPolicyStoreFindsWhatItSaved()
    {
        var store = FeedbackPolicyStore.NewMemoryFeedbackPolicyStore();
        var item = FeedbackPolicy.NewFeedbackPolicy("code-489", "title-886");
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
        MemoryFeedbackPolicyStoreFindsWhatItSaved();
    }
}
