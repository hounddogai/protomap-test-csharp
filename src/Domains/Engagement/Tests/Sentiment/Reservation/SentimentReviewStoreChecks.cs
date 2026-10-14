namespace Acme.Workforce.Engagement.Tests.Sentiment.Reservation;

using Acme.Workforce.Engagement.Sentiment.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class SentimentReviewStoreChecks
{
    public static void MemorySentimentReviewStoreFindsWhatItSaved()
    {
        var store = SentimentReviewStore.NewMemorySentimentReviewStore();
        var item = SentimentReview.NewSentimentReview("code-167", "name-379");
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
        MemorySentimentReviewStoreFindsWhatItSaved();
    }
}
