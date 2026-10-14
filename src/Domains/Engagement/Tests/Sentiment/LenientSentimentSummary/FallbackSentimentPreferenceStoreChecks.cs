namespace Acme.Workforce.Engagement.Tests.Sentiment.LenientSentimentSummary;

using Acme.Workforce.Engagement.Sentiment.LenientSentimentSummary;

/// <summary>Checks of lenient sentiment summary code, run by the project's runner.</summary>
public static class FallbackSentimentPreferenceStoreChecks
{
    public static void MemoryFallbackSentimentPreferenceStoreFindsWhatItSaved()
    {
        var store = FallbackSentimentPreferenceStore.NewMemoryFallbackSentimentPreferenceStore();
        var item = FallbackSentimentPreference.NewFallbackSentimentPreference("code-704", "title-412");
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
        MemoryFallbackSentimentPreferenceStoreFindsWhatItSaved();
    }
}
