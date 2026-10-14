namespace Acme.Workforce.Engagement.Tests.Feedback.Score;

using Acme.Workforce.Engagement.Feedback.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class GlobalFeedbackVarianceStoreChecks
{
    public static void MemoryGlobalFeedbackVarianceStoreFindsWhatItSaved()
    {
        var store = GlobalFeedbackVarianceStore.NewMemoryGlobalFeedbackVarianceStore();
        var item = GlobalFeedbackVariance.NewGlobalFeedbackVariance("code-851", "name-73");
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
        MemoryGlobalFeedbackVarianceStoreFindsWhatItSaved();
    }
}
