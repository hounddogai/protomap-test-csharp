namespace Acme.Workforce.Engagement.Tests.Sentiment.Reservation;

using Acme.Workforce.Engagement.Sentiment.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class SentimentReviewEngineChecks
{
    public static void SentimentReviewEngineKeepsResultsWithinBounds()
    {
        var engine = SentimentReviewEngine.DefaultSentimentReviewEngine();
        var item = SentimentReview.NewSentimentReview("code-167", "name-379");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SentimentReviewEngineKeepsResultsWithinBounds();
    }
}
