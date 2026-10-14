namespace Acme.Workforce.Engagement.Tests.Sentiment.Variance;

using Acme.Workforce.Engagement.Sentiment.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class PendingSentimentVarianceStoreChecks
{
    public static void MemoryPendingSentimentVarianceStoreFindsWhatItSaved()
    {
        var store = PendingSentimentVarianceStore.NewMemoryPendingSentimentVarianceStore();
        var item = PendingSentimentVariance.NewPendingSentimentVariance("code-242", "name-337");
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
        MemoryPendingSentimentVarianceStoreFindsWhatItSaved();
    }
}
