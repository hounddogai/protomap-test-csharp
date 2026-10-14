namespace Acme.Workforce.Engagement.Tests.Sentiment.Exception;

using Acme.Workforce.Engagement.Sentiment.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class RegionalSentimentSnapshotStoreChecks
{
    public static void MemoryRegionalSentimentSnapshotStoreFindsWhatItSaved()
    {
        var store = RegionalSentimentSnapshotStore.NewMemoryRegionalSentimentSnapshotStore();
        var item = RegionalSentimentSnapshot.NewRegionalSentimentSnapshot("code-411", "label-926");
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
        MemoryRegionalSentimentSnapshotStoreFindsWhatItSaved();
    }
}
