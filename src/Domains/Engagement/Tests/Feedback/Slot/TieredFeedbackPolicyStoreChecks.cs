namespace Acme.Workforce.Engagement.Tests.Feedback.Slot;

using Acme.Workforce.Engagement.Feedback.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class TieredFeedbackPolicyStoreChecks
{
    public static void MemoryTieredFeedbackPolicyStoreFindsWhatItSaved()
    {
        var store = TieredFeedbackPolicyStore.NewMemoryTieredFeedbackPolicyStore();
        var item = TieredFeedbackPolicy.NewTieredFeedbackPolicy("code-563", "title-669");
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
        MemoryTieredFeedbackPolicyStoreFindsWhatItSaved();
    }
}
