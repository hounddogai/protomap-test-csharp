namespace Acme.Workforce.Engagement.Tests.Sentiment.Adjustment;

using Acme.Workforce.Engagement.Sentiment.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class SentimentCheckpointChecks
{
    public static void SentimentCheckpointStartsInPlanned()
    {
        var item = SentimentCheckpoint.NewSentimentCheckpoint("code-751", "name-633");
        Check.Equal(item.Status, SentimentCheckpointStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SentimentCheckpointMovesToInProgress()
    {
        var item = SentimentCheckpoint.NewSentimentCheckpoint("code-751", "name-633");
        try
        {
            item.MoveTo(SentimentCheckpointStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SentimentCheckpointStatus.InProgress, "status");
    }

    public static void SentimentCheckpointRejectsMovesFromCancelled()
    {
        var item = SentimentCheckpoint.NewSentimentCheckpoint("code-751", "name-633");
        item.Status = SentimentCheckpointStatus.Cancelled;
        Check.Fails(() => item.MoveTo(SentimentCheckpointStatus.Planned), "move");
    }

    public static void RunAll()
    {
        SentimentCheckpointStartsInPlanned();
        SentimentCheckpointMovesToInProgress();
        SentimentCheckpointRejectsMovesFromCancelled();
    }
}
