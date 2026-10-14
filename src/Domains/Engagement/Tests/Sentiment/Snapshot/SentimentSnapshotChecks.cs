namespace Acme.Workforce.Engagement.Tests.Sentiment.Snapshot;

using Acme.Workforce.Engagement.Sentiment.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class SentimentSnapshotChecks
{
    public static void SentimentSnapshotStartsInOpen()
    {
        var item = SentimentSnapshot.NewSentimentSnapshot("code-274", "title-498");
        Check.Equal(item.Status, SentimentSnapshotStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SentimentSnapshotMovesToArchived()
    {
        var item = SentimentSnapshot.NewSentimentSnapshot("code-274", "title-498");
        try
        {
            item.MoveTo(SentimentSnapshotStatus.Archived);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SentimentSnapshotStatus.Archived, "status");
    }

    public static void SentimentSnapshotRejectsMovesFromArchived()
    {
        var item = SentimentSnapshot.NewSentimentSnapshot("code-274", "title-498");
        item.Status = SentimentSnapshotStatus.Archived;
        Check.Fails(() => item.MoveTo(SentimentSnapshotStatus.Open), "move");
    }

    public static void RunAll()
    {
        SentimentSnapshotStartsInOpen();
        SentimentSnapshotMovesToArchived();
        SentimentSnapshotRejectsMovesFromArchived();
    }
}
