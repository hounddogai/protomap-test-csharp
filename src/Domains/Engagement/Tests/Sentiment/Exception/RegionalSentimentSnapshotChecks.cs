namespace Acme.Workforce.Engagement.Tests.Sentiment.Exception;

using Acme.Workforce.Engagement.Sentiment.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class RegionalSentimentSnapshotChecks
{
    public static void RegionalSentimentSnapshotStartsInOpen()
    {
        var item = RegionalSentimentSnapshot.NewRegionalSentimentSnapshot("code-411", "label-926");
        Check.Equal(item.Status, RegionalSentimentSnapshotStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalSentimentSnapshotMovesToInReview()
    {
        var item = RegionalSentimentSnapshot.NewRegionalSentimentSnapshot("code-411", "label-926");
        try
        {
            item.MoveTo(RegionalSentimentSnapshotStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalSentimentSnapshotStatus.InReview, "status");
    }

    public static void RegionalSentimentSnapshotRejectsMovesFromArchived()
    {
        var item = RegionalSentimentSnapshot.NewRegionalSentimentSnapshot("code-411", "label-926");
        item.Status = RegionalSentimentSnapshotStatus.Archived;
        Check.Fails(() => item.MoveTo(RegionalSentimentSnapshotStatus.Open), "move");
    }

    public static void RunAll()
    {
        RegionalSentimentSnapshotStartsInOpen();
        RegionalSentimentSnapshotMovesToInReview();
        RegionalSentimentSnapshotRejectsMovesFromArchived();
    }
}
