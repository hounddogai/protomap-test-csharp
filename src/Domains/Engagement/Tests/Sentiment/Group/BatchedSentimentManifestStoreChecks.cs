namespace Acme.Workforce.Engagement.Tests.Sentiment.Group;

using Acme.Workforce.Engagement.Sentiment.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class BatchedSentimentManifestStoreChecks
{
    public static void MemoryBatchedSentimentManifestStoreFindsWhatItSaved()
    {
        var store = BatchedSentimentManifestStore.NewMemoryBatchedSentimentManifestStore();
        var item = BatchedSentimentManifest.NewBatchedSentimentManifest("code-314", "name-707");
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
        MemoryBatchedSentimentManifestStoreFindsWhatItSaved();
    }
}
