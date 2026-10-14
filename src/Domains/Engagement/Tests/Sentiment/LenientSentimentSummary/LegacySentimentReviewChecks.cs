namespace Acme.Workforce.Engagement.Tests.Sentiment.LenientSentimentSummary;

using Acme.Workforce.Engagement.Sentiment.LenientSentimentSummary;

/// <summary>Checks of lenient sentiment summary code, run by the project's runner.</summary>
public static class LegacySentimentReviewChecks
{
    public static void LegacySentimentReviewStartsInPlanned()
    {
        var item = LegacySentimentReview.NewLegacySentimentReview("code-906", "title-233");
        Check.Equal(item.Status, LegacySentimentReviewStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacySentimentReviewMovesToScheduled()
    {
        var item = LegacySentimentReview.NewLegacySentimentReview("code-906", "title-233");
        try
        {
            item.MoveTo(LegacySentimentReviewStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacySentimentReviewStatus.Scheduled, "status");
    }

    public static void LegacySentimentReviewRejectsMovesFromCancelled()
    {
        var item = LegacySentimentReview.NewLegacySentimentReview("code-906", "title-233");
        item.Status = LegacySentimentReviewStatus.Cancelled;
        Check.Fails(() => item.MoveTo(LegacySentimentReviewStatus.Planned), "move");
    }

    public static void RunAll()
    {
        LegacySentimentReviewStartsInPlanned();
        LegacySentimentReviewMovesToScheduled();
        LegacySentimentReviewRejectsMovesFromCancelled();
    }
}
