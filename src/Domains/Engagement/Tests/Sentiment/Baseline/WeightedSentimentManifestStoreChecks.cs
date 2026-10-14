namespace Acme.Workforce.Engagement.Tests.Sentiment.Baseline;

using Acme.Workforce.Engagement.Sentiment.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class WeightedSentimentManifestStoreChecks
{
    public static void MemoryWeightedSentimentManifestStoreFindsWhatItSaved()
    {
        var store = WeightedSentimentManifestStore.NewMemoryWeightedSentimentManifestStore();
        var item = WeightedSentimentManifest.NewWeightedSentimentManifest("code-838", "title-174");
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
        MemoryWeightedSentimentManifestStoreFindsWhatItSaved();
    }
}
