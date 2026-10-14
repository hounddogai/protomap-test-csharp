namespace Acme.Workforce.Engagement.Tests.Sentiment.Reservation;

using Acme.Workforce.Engagement.Sentiment.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class SentimentReviewChecks
{
    public static void SentimentReviewStartsInIdle()
    {
        var item = SentimentReview.NewSentimentReview("code-167", "name-379");
        Check.Equal(item.Status, SentimentReviewStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SentimentReviewMovesToStopped()
    {
        var item = SentimentReview.NewSentimentReview("code-167", "name-379");
        try
        {
            item.MoveTo(SentimentReviewStatus.Stopped);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SentimentReviewStatus.Stopped, "status");
    }

    public static void SentimentReviewRejectsMovesFromStopped()
    {
        var item = SentimentReview.NewSentimentReview("code-167", "name-379");
        item.Status = SentimentReviewStatus.Stopped;
        Check.Fails(() => item.MoveTo(SentimentReviewStatus.Idle), "move");
    }

    public static void RunAll()
    {
        SentimentReviewStartsInIdle();
        SentimentReviewMovesToStopped();
        SentimentReviewRejectsMovesFromStopped();
    }
}
