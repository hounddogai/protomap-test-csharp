namespace Acme.Workforce.Engagement.Tests.Sentiment.SentimentManifest;

using Acme.Workforce.Engagement.Sentiment.SentimentManifest;

/// <summary>Checks of sentiment manifest code, run by the project's runner.</summary>
public static class TieredSentimentManifestChecks
{
    public static void TieredSentimentManifestStartsInNew()
    {
        var item = TieredSentimentManifest.NewTieredSentimentManifest("code-148", "name-963");
        Check.Equal(item.Status, TieredSentimentManifestStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredSentimentManifestMovesToQueued()
    {
        var item = TieredSentimentManifest.NewTieredSentimentManifest("code-148", "name-963");
        try
        {
            item.MoveTo(TieredSentimentManifestStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredSentimentManifestStatus.Queued, "status");
    }

    public static void TieredSentimentManifestRejectsMovesFromFailed()
    {
        var item = TieredSentimentManifest.NewTieredSentimentManifest("code-148", "name-963");
        item.Status = TieredSentimentManifestStatus.Failed;
        Check.Fails(() => item.MoveTo(TieredSentimentManifestStatus.New), "move");
    }

    public static void RunAll()
    {
        TieredSentimentManifestStartsInNew();
        TieredSentimentManifestMovesToQueued();
        TieredSentimentManifestRejectsMovesFromFailed();
    }
}
