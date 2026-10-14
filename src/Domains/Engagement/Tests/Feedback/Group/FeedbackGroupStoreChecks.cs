namespace Acme.Workforce.Engagement.Tests.Feedback.Group;

using Acme.Workforce.Engagement.Feedback.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class FeedbackGroupStoreChecks
{
    public static void MemoryFeedbackGroupStoreFindsWhatItSaved()
    {
        var store = FeedbackGroupStore.NewMemoryFeedbackGroupStore();
        var item = FeedbackGroup.NewFeedbackGroup("code-862", "name-565");
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
        MemoryFeedbackGroupStoreFindsWhatItSaved();
    }
}
