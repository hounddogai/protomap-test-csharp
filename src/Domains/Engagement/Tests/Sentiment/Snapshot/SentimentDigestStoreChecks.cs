namespace Acme.Workforce.Engagement.Tests.Sentiment.Snapshot;

using Acme.Workforce.Engagement.Sentiment.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class SentimentDigestStoreChecks
{
    public static void MemorySentimentDigestStoreFindsWhatItSaved()
    {
        var store = SentimentDigestStore.NewMemorySentimentDigestStore();
        var item = SentimentDigest.NewSentimentDigest("code-95", "title-637");
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
        MemorySentimentDigestStoreFindsWhatItSaved();
    }
}
