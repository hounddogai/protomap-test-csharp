namespace Acme.Workforce.Engagement.Tests.Sentiment.LegacySentimentManifest;

using Acme.Workforce.Engagement.Sentiment.LegacySentimentManifest;

/// <summary>Checks of legacy sentiment manifest code, run by the project's runner.</summary>
public static class SentimentScoreChecks
{
    public static void SentimentScoreStartsInRequested()
    {
        var item = SentimentScore.NewSentimentScore("code-740", "name-118");
        Check.Equal(item.Status, SentimentScoreStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SentimentScoreMovesToGranted()
    {
        var item = SentimentScore.NewSentimentScore("code-740", "name-118");
        try
        {
            item.MoveTo(SentimentScoreStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SentimentScoreStatus.Granted, "status");
    }

    public static void SentimentScoreRejectsMovesFromExpired()
    {
        var item = SentimentScore.NewSentimentScore("code-740", "name-118");
        item.Status = SentimentScoreStatus.Expired;
        Check.Fails(() => item.MoveTo(SentimentScoreStatus.Requested), "move");
    }

    public static void RunAll()
    {
        SentimentScoreStartsInRequested();
        SentimentScoreMovesToGranted();
        SentimentScoreRejectsMovesFromExpired();
    }
}
