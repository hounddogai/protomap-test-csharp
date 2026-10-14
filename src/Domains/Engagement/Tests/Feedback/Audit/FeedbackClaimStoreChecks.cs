namespace Acme.Workforce.Engagement.Tests.Feedback.Audit;

using Acme.Workforce.Engagement.Feedback.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class FeedbackClaimStoreChecks
{
    public static void MemoryFeedbackClaimStoreFindsWhatItSaved()
    {
        var store = FeedbackClaimStore.NewMemoryFeedbackClaimStore();
        var item = FeedbackClaim.NewFeedbackClaim("code-636", "name-516");
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
        MemoryFeedbackClaimStoreFindsWhatItSaved();
    }
}
