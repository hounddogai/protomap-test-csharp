namespace Acme.Workforce.Engagement.Tests.Sentiment.SentimentVariance;

using Acme.Workforce.Engagement.Sentiment.SentimentVariance;

/// <summary>Checks of sentiment variance code, run by the project's runner.</summary>
public static class ManualSentimentVarianceStoreChecks
{
    public static void MemoryManualSentimentVarianceStoreFindsWhatItSaved()
    {
        var store = ManualSentimentVarianceStore.NewMemoryManualSentimentVarianceStore();
        var item = ManualSentimentVariance.NewManualSentimentVariance("code-757", "label-858");
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
        MemoryManualSentimentVarianceStoreFindsWhatItSaved();
    }
}
