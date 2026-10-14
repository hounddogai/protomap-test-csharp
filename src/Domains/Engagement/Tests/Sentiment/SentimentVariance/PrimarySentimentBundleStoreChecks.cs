namespace Acme.Workforce.Engagement.Tests.Sentiment.SentimentVariance;

using Acme.Workforce.Engagement.Sentiment.SentimentVariance;

/// <summary>Checks of sentiment variance code, run by the project's runner.</summary>
public static class PrimarySentimentBundleStoreChecks
{
    public static void MemoryPrimarySentimentBundleStoreFindsWhatItSaved()
    {
        var store = PrimarySentimentBundleStore.NewMemoryPrimarySentimentBundleStore();
        var item = PrimarySentimentBundle.NewPrimarySentimentBundle("code-703", "title-911");
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
        MemoryPrimarySentimentBundleStoreFindsWhatItSaved();
    }
}
