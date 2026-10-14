namespace Acme.Workforce.Engagement.Tests.Feedback.FeedbackScore;

using Acme.Workforce.Engagement.Feedback.FeedbackScore;

/// <summary>Checks of feedback score code, run by the project's runner.</summary>
public static class SeasonalFeedbackScoreStoreChecks
{
    public static void MemorySeasonalFeedbackScoreStoreFindsWhatItSaved()
    {
        var store = SeasonalFeedbackScoreStore.NewMemorySeasonalFeedbackScoreStore();
        var item = SeasonalFeedbackScore.NewSeasonalFeedbackScore("code-214", "title-530");
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
        MemorySeasonalFeedbackScoreStoreFindsWhatItSaved();
    }
}
