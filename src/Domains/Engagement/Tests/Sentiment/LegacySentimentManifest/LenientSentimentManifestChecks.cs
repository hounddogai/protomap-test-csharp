namespace Acme.Workforce.Engagement.Tests.Sentiment.LegacySentimentManifest;

using Acme.Workforce.Engagement.Sentiment.LegacySentimentManifest;

/// <summary>Checks of legacy sentiment manifest code, run by the project's runner.</summary>
public static class LenientSentimentManifestChecks
{
    public static void LenientSentimentManifestStartsInIdle()
    {
        var item = LenientSentimentManifest.NewLenientSentimentManifest("code-846", "title-24");
        Check.Equal(item.Status, LenientSentimentManifestStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientSentimentManifestMovesToReady()
    {
        var item = LenientSentimentManifest.NewLenientSentimentManifest("code-846", "title-24");
        try
        {
            item.MoveTo(LenientSentimentManifestStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientSentimentManifestStatus.Ready, "status");
    }

    public static void LenientSentimentManifestRejectsMovesFromStopped()
    {
        var item = LenientSentimentManifest.NewLenientSentimentManifest("code-846", "title-24");
        item.Status = LenientSentimentManifestStatus.Stopped;
        Check.Fails(() => item.MoveTo(LenientSentimentManifestStatus.Idle), "move");
    }

    public static void RunAll()
    {
        LenientSentimentManifestStartsInIdle();
        LenientSentimentManifestMovesToReady();
        LenientSentimentManifestRejectsMovesFromStopped();
    }
}
