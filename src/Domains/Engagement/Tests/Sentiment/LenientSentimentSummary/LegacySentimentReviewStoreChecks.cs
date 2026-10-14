namespace Acme.Workforce.Engagement.Tests.Sentiment.LenientSentimentSummary;

using Acme.Workforce.Engagement.Sentiment.LenientSentimentSummary;

/// <summary>Checks of lenient sentiment summary code, run by the project's runner.</summary>
public static class LegacySentimentReviewStoreChecks
{
    public static void MemoryLegacySentimentReviewStoreFindsWhatItSaved()
    {
        var store = LegacySentimentReviewStore.NewMemoryLegacySentimentReviewStore();
        var item = LegacySentimentReview.NewLegacySentimentReview("code-906", "title-233");
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
        MemoryLegacySentimentReviewStoreFindsWhatItSaved();
    }
}
