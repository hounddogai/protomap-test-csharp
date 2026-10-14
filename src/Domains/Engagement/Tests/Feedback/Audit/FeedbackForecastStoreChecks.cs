namespace Acme.Workforce.Engagement.Tests.Feedback.Audit;

using Acme.Workforce.Engagement.Feedback.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class FeedbackForecastStoreChecks
{
    public static void MemoryFeedbackForecastStoreFindsWhatItSaved()
    {
        var store = FeedbackForecastStore.NewMemoryFeedbackForecastStore();
        var item = FeedbackForecast.NewFeedbackForecast("code-987", "label-830");
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
        MemoryFeedbackForecastStoreFindsWhatItSaved();
    }
}
