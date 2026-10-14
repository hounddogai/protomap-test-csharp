namespace Acme.Workforce.Engagement.Tests.Sentiment.SentimentSummary;

using Acme.Workforce.Engagement.Sentiment.SentimentSummary;

/// <summary>Checks of sentiment summary code, run by the project's runner.</summary>
public static class SentimentBundleStoreChecks
{
    public static void MemorySentimentBundleStoreFindsWhatItSaved()
    {
        var store = SentimentBundleStore.NewMemorySentimentBundleStore();
        var item = SentimentBundle.NewSentimentBundle("code-423", "name-579");
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
        MemorySentimentBundleStoreFindsWhatItSaved();
    }
}
