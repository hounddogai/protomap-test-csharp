namespace Acme.Workforce.Engagement.Tests.Sentiment.LenientSentimentSummary;

using Acme.Workforce.Engagement.Sentiment.LenientSentimentSummary;

/// <summary>Checks of lenient sentiment summary code, run by the project's runner.</summary>
public static class SentimentTierStoreChecks
{
    public static void MemorySentimentTierStoreFindsWhatItSaved()
    {
        var store = SentimentTierStore.NewMemorySentimentTierStore();
        var item = SentimentTier.NewSentimentTier("code-167", "name-928");
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
        MemorySentimentTierStoreFindsWhatItSaved();
    }
}
