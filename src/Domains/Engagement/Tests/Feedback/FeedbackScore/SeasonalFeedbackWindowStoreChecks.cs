namespace Acme.Workforce.Engagement.Tests.Feedback.FeedbackScore;

using Acme.Workforce.Engagement.Feedback.FeedbackScore;

/// <summary>Checks of feedback score code, run by the project's runner.</summary>
public static class SeasonalFeedbackWindowStoreChecks
{
    public static void MemorySeasonalFeedbackWindowStoreFindsWhatItSaved()
    {
        var store = SeasonalFeedbackWindowStore.NewMemorySeasonalFeedbackWindowStore();
        var item = SeasonalFeedbackWindow.NewSeasonalFeedbackWindow("code-426", "name-731");
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
        MemorySeasonalFeedbackWindowStoreFindsWhatItSaved();
    }
}
