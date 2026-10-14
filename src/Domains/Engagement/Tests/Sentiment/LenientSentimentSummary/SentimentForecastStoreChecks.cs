namespace Acme.Workforce.Engagement.Tests.Sentiment.LenientSentimentSummary;

using Acme.Workforce.Engagement.Sentiment.LenientSentimentSummary;

/// <summary>Checks of lenient sentiment summary code, run by the project's runner.</summary>
public static class SentimentForecastStoreChecks
{
    public static void MemorySentimentForecastStoreFindsWhatItSaved()
    {
        var store = SentimentForecastStore.NewMemorySentimentForecastStore();
        var item = SentimentForecast.NewSentimentForecast("code-533", "title-299");
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
        MemorySentimentForecastStoreFindsWhatItSaved();
    }
}
