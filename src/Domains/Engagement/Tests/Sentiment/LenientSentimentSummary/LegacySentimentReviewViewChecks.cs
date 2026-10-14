namespace Acme.Workforce.Engagement.Tests.Sentiment.LenientSentimentSummary;

using Acme.Workforce.Engagement.Sentiment.LenientSentimentSummary;

/// <summary>Checks of lenient sentiment summary code, run by the project's runner.</summary>
public static class LegacySentimentReviewViewChecks
{
    public static void LegacySentimentReviewViewShowsTheStatusInCapitals()
    {
        var item = LegacySentimentReview.NewLegacySentimentReview("code-906", "title-233");
        var view = LegacySentimentReviewView.ToLegacySentimentReviewView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LegacySentimentReviewView.ToLegacySentimentReviewViews(new List<LegacySentimentReview> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LegacySentimentReviewViewShowsTheStatusInCapitals();
    }
}
