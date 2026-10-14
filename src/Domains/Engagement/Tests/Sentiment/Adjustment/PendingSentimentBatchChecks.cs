namespace Acme.Workforce.Engagement.Tests.Sentiment.Adjustment;

using Acme.Workforce.Engagement.Sentiment.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class PendingSentimentBatchChecks
{
    public static void PendingSentimentBatchStartsInNew()
    {
        var item = PendingSentimentBatch.NewPendingSentimentBatch("code-343", "name-803");
        Check.Equal(item.Status, PendingSentimentBatchStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingSentimentBatchMovesToQueued()
    {
        var item = PendingSentimentBatch.NewPendingSentimentBatch("code-343", "name-803");
        try
        {
            item.MoveTo(PendingSentimentBatchStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingSentimentBatchStatus.Queued, "status");
    }

    public static void PendingSentimentBatchRejectsMovesFromFailed()
    {
        var item = PendingSentimentBatch.NewPendingSentimentBatch("code-343", "name-803");
        item.Status = PendingSentimentBatchStatus.Failed;
        Check.Fails(() => item.MoveTo(PendingSentimentBatchStatus.New), "move");
    }

    public static void RunAll()
    {
        PendingSentimentBatchStartsInNew();
        PendingSentimentBatchMovesToQueued();
        PendingSentimentBatchRejectsMovesFromFailed();
    }
}
