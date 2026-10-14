namespace Acme.Workforce.Engagement.Tests.Sentiment.Exception;

using Acme.Workforce.Engagement.Sentiment.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class GlobalSentimentBundleStoreChecks
{
    public static void MemoryGlobalSentimentBundleStoreFindsWhatItSaved()
    {
        var store = GlobalSentimentBundleStore.NewMemoryGlobalSentimentBundleStore();
        var item = GlobalSentimentBundle.NewGlobalSentimentBundle("code-374", "name-222");
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
        MemoryGlobalSentimentBundleStoreFindsWhatItSaved();
    }
}
