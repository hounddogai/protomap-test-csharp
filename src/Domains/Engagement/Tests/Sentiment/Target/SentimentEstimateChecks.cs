namespace Acme.Workforce.Engagement.Tests.Sentiment.Target;

using Acme.Workforce.Engagement.Sentiment.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class SentimentEstimateChecks
{
    public static void SentimentEstimateStartsInRequested()
    {
        var item = SentimentEstimate.NewSentimentEstimate("code-885", "label-545");
        Check.Equal(item.Status, SentimentEstimateStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SentimentEstimateMovesToGranted()
    {
        var item = SentimentEstimate.NewSentimentEstimate("code-885", "label-545");
        try
        {
            item.MoveTo(SentimentEstimateStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SentimentEstimateStatus.Granted, "status");
    }

    public static void SentimentEstimateRejectsMovesFromExpired()
    {
        var item = SentimentEstimate.NewSentimentEstimate("code-885", "label-545");
        item.Status = SentimentEstimateStatus.Expired;
        Check.Fails(() => item.MoveTo(SentimentEstimateStatus.Requested), "move");
    }

    public static void RunAll()
    {
        SentimentEstimateStartsInRequested();
        SentimentEstimateMovesToGranted();
        SentimentEstimateRejectsMovesFromExpired();
    }
}
