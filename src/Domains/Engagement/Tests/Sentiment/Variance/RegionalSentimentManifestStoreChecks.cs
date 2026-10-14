namespace Acme.Workforce.Engagement.Tests.Sentiment.Variance;

using Acme.Workforce.Engagement.Sentiment.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class RegionalSentimentManifestStoreChecks
{
    public static void MemoryRegionalSentimentManifestStoreFindsWhatItSaved()
    {
        var store = RegionalSentimentManifestStore.NewMemoryRegionalSentimentManifestStore();
        var item = RegionalSentimentManifest.NewRegionalSentimentManifest("code-64", "name-23");
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
        MemoryRegionalSentimentManifestStoreFindsWhatItSaved();
    }
}
