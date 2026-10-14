namespace Acme.Workforce.Engagement.Tests.Sentiment.Milestone;

using Acme.Workforce.Engagement.Sentiment.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class SentimentBatchChecks
{
    public static void SentimentBatchStartsInIdle()
    {
        var item = SentimentBatch.NewSentimentBatch("code-32", "title-89");
        Check.Equal(item.Status, SentimentBatchStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SentimentBatchMovesToDraining()
    {
        var item = SentimentBatch.NewSentimentBatch("code-32", "title-89");
        try
        {
            item.MoveTo(SentimentBatchStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SentimentBatchStatus.Draining, "status");
    }

    public static void SentimentBatchRejectsMovesFromStopped()
    {
        var item = SentimentBatch.NewSentimentBatch("code-32", "title-89");
        item.Status = SentimentBatchStatus.Stopped;
        Check.Fails(() => item.MoveTo(SentimentBatchStatus.Idle), "move");
    }

    public static void RunAll()
    {
        SentimentBatchStartsInIdle();
        SentimentBatchMovesToDraining();
        SentimentBatchRejectsMovesFromStopped();
    }
}
