namespace Acme.Workforce.Engagement.Tests.Sentiment.Proposal;

using Acme.Workforce.Engagement.Sentiment.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class SentimentThresholdChecks
{
    public static void SentimentThresholdStartsInRequested()
    {
        var item = SentimentThreshold.NewSentimentThreshold("code-661", "title-678");
        Check.Equal(item.Status, SentimentThresholdStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SentimentThresholdMovesToRevoked()
    {
        var item = SentimentThreshold.NewSentimentThreshold("code-661", "title-678");
        try
        {
            item.MoveTo(SentimentThresholdStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SentimentThresholdStatus.Revoked, "status");
    }

    public static void SentimentThresholdRejectsMovesFromExpired()
    {
        var item = SentimentThreshold.NewSentimentThreshold("code-661", "title-678");
        item.Status = SentimentThresholdStatus.Expired;
        Check.Fails(() => item.MoveTo(SentimentThresholdStatus.Requested), "move");
    }

    public static void RunAll()
    {
        SentimentThresholdStartsInRequested();
        SentimentThresholdMovesToRevoked();
        SentimentThresholdRejectsMovesFromExpired();
    }
}
