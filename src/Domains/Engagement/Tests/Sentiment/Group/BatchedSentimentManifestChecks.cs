namespace Acme.Workforce.Engagement.Tests.Sentiment.Group;

using Acme.Workforce.Engagement.Sentiment.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class BatchedSentimentManifestChecks
{
    public static void BatchedSentimentManifestStartsInRequested()
    {
        var item = BatchedSentimentManifest.NewBatchedSentimentManifest("code-314", "name-707");
        Check.Equal(item.Status, BatchedSentimentManifestStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedSentimentManifestMovesToGranted()
    {
        var item = BatchedSentimentManifest.NewBatchedSentimentManifest("code-314", "name-707");
        try
        {
            item.MoveTo(BatchedSentimentManifestStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedSentimentManifestStatus.Granted, "status");
    }

    public static void BatchedSentimentManifestRejectsMovesFromExpired()
    {
        var item = BatchedSentimentManifest.NewBatchedSentimentManifest("code-314", "name-707");
        item.Status = BatchedSentimentManifestStatus.Expired;
        Check.Fails(() => item.MoveTo(BatchedSentimentManifestStatus.Requested), "move");
    }

    public static void RunAll()
    {
        BatchedSentimentManifestStartsInRequested();
        BatchedSentimentManifestMovesToGranted();
        BatchedSentimentManifestRejectsMovesFromExpired();
    }
}
