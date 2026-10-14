namespace Acme.Workforce.Engagement.Tests.Feedback.Summary;

using Acme.Workforce.Engagement.Feedback.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class DefaultFeedbackTargetStoreChecks
{
    public static void MemoryDefaultFeedbackTargetStoreFindsWhatItSaved()
    {
        var store = DefaultFeedbackTargetStore.NewMemoryDefaultFeedbackTargetStore();
        var item = DefaultFeedbackTarget.NewDefaultFeedbackTarget("code-712", "title-483");
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
        MemoryDefaultFeedbackTargetStoreFindsWhatItSaved();
    }
}
