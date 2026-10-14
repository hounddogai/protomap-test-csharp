namespace Acme.Workforce.Engagement.Tests.Sentiment.Threshold;

using Acme.Workforce.Engagement.Sentiment.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class FallbackSentimentScoreChecks
{
    public static void FallbackSentimentScoreStartsInDraft()
    {
        var item = FallbackSentimentScore.NewFallbackSentimentScore("code-978", "label-461");
        Check.Equal(item.Status, FallbackSentimentScoreStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackSentimentScoreMovesToActive()
    {
        var item = FallbackSentimentScore.NewFallbackSentimentScore("code-978", "label-461");
        try
        {
            item.MoveTo(FallbackSentimentScoreStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackSentimentScoreStatus.Active, "status");
    }

    public static void FallbackSentimentScoreRejectsMovesFromClosed()
    {
        var item = FallbackSentimentScore.NewFallbackSentimentScore("code-978", "label-461");
        item.Status = FallbackSentimentScoreStatus.Closed;
        Check.Fails(() => item.MoveTo(FallbackSentimentScoreStatus.Draft), "move");
    }

    public static void RunAll()
    {
        FallbackSentimentScoreStartsInDraft();
        FallbackSentimentScoreMovesToActive();
        FallbackSentimentScoreRejectsMovesFromClosed();
    }
}
