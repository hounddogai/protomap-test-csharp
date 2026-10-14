namespace Acme.Workforce.Engagement.Tests.Sentiment.Rollup;

using Acme.Workforce.Engagement.Sentiment.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class CappedSentimentManifestChecks
{
    public static void CappedSentimentManifestStartsInProposed()
    {
        var item = CappedSentimentManifest.NewCappedSentimentManifest("code-834", "title-310");
        Check.Equal(item.Status, CappedSentimentManifestStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedSentimentManifestMovesToWithdrawn()
    {
        var item = CappedSentimentManifest.NewCappedSentimentManifest("code-834", "title-310");
        try
        {
            item.MoveTo(CappedSentimentManifestStatus.Withdrawn);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedSentimentManifestStatus.Withdrawn, "status");
    }

    public static void CappedSentimentManifestRejectsMovesFromWithdrawn()
    {
        var item = CappedSentimentManifest.NewCappedSentimentManifest("code-834", "title-310");
        item.Status = CappedSentimentManifestStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(CappedSentimentManifestStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        CappedSentimentManifestStartsInProposed();
        CappedSentimentManifestMovesToWithdrawn();
        CappedSentimentManifestRejectsMovesFromWithdrawn();
    }
}
