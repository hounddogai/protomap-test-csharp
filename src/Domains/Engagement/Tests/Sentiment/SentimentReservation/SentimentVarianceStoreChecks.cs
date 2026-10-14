namespace Acme.Workforce.Engagement.Tests.Sentiment.SentimentReservation;

using Acme.Workforce.Engagement.Sentiment.SentimentReservation;

/// <summary>Checks of sentiment reservation code, run by the project's runner.</summary>
public static class SentimentVarianceStoreChecks
{
    public static void MemorySentimentVarianceStoreFindsWhatItSaved()
    {
        var store = SentimentVarianceStore.NewMemorySentimentVarianceStore();
        var item = SentimentVariance.NewSentimentVariance("code-677", "title-770");
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
        MemorySentimentVarianceStoreFindsWhatItSaved();
    }
}
