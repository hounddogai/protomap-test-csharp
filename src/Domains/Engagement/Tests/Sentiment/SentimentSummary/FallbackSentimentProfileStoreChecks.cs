namespace Acme.Workforce.Engagement.Tests.Sentiment.SentimentSummary;

using Acme.Workforce.Engagement.Sentiment.SentimentSummary;

/// <summary>Checks of sentiment summary code, run by the project's runner.</summary>
public static class FallbackSentimentProfileStoreChecks
{
    public static void MemoryFallbackSentimentProfileStoreFindsWhatItSaved()
    {
        var store = FallbackSentimentProfileStore.NewMemoryFallbackSentimentProfileStore();
        var item = FallbackSentimentProfile.NewFallbackSentimentProfile("code-4", "title-374");
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
        MemoryFallbackSentimentProfileStoreFindsWhatItSaved();
    }
}
