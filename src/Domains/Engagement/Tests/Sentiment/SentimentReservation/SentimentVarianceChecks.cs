namespace Acme.Workforce.Engagement.Tests.Sentiment.SentimentReservation;

using Acme.Workforce.Engagement.Sentiment.SentimentReservation;

/// <summary>Checks of sentiment reservation code, run by the project's runner.</summary>
public static class SentimentVarianceChecks
{
    public static void SentimentVarianceStartsInPlanned()
    {
        var item = SentimentVariance.NewSentimentVariance("code-677", "title-770");
        Check.Equal(item.Status, SentimentVarianceStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SentimentVarianceMovesToScheduled()
    {
        var item = SentimentVariance.NewSentimentVariance("code-677", "title-770");
        try
        {
            item.MoveTo(SentimentVarianceStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SentimentVarianceStatus.Scheduled, "status");
    }

    public static void SentimentVarianceRejectsMovesFromCancelled()
    {
        var item = SentimentVariance.NewSentimentVariance("code-677", "title-770");
        item.Status = SentimentVarianceStatus.Cancelled;
        Check.Fails(() => item.MoveTo(SentimentVarianceStatus.Planned), "move");
    }

    public static void RunAll()
    {
        SentimentVarianceStartsInPlanned();
        SentimentVarianceMovesToScheduled();
        SentimentVarianceRejectsMovesFromCancelled();
    }
}
