namespace Acme.Workforce.Engagement.Tests.Sentiment.Rollup;

using Acme.Workforce.Engagement.Sentiment.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class CappedSentimentManifestStoreChecks
{
    public static void MemoryCappedSentimentManifestStoreFindsWhatItSaved()
    {
        var store = CappedSentimentManifestStore.NewMemoryCappedSentimentManifestStore();
        var item = CappedSentimentManifest.NewCappedSentimentManifest("code-834", "title-310");
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
        MemoryCappedSentimentManifestStoreFindsWhatItSaved();
    }
}
