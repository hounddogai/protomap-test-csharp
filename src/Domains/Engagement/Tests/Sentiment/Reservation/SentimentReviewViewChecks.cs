namespace Acme.Workforce.Engagement.Tests.Sentiment.Reservation;

using Acme.Workforce.Engagement.Sentiment.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class SentimentReviewViewChecks
{
    public static void SentimentReviewViewShowsTheStatusInCapitals()
    {
        var item = SentimentReview.NewSentimentReview("code-167", "name-379");
        var view = SentimentReviewView.ToSentimentReviewView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SentimentReviewView.ToSentimentReviewViews(new List<SentimentReview> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SentimentReviewViewShowsTheStatusInCapitals();
    }
}
