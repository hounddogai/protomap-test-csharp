namespace Acme.Workforce.Engagement.Tests.Sentiment.SentimentManifest;

using Acme.Workforce.Engagement.Sentiment.SentimentManifest;

/// <summary>Checks of sentiment manifest code, run by the project's runner.</summary>
public static class TieredSentimentManifestStoreChecks
{
    public static void MemoryTieredSentimentManifestStoreFindsWhatItSaved()
    {
        var store = TieredSentimentManifestStore.NewMemoryTieredSentimentManifestStore();
        var item = TieredSentimentManifest.NewTieredSentimentManifest("code-148", "name-963");
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
        MemoryTieredSentimentManifestStoreFindsWhatItSaved();
    }
}
