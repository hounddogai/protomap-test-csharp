namespace Acme.Workforce.Engagement.Tests.Sentiment.Variance;

using Acme.Workforce.Engagement.Sentiment.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class RegionalSentimentManifestChecks
{
    public static void RegionalSentimentManifestStartsInPlanned()
    {
        var item = RegionalSentimentManifest.NewRegionalSentimentManifest("code-64", "name-23");
        Check.Equal(item.Status, RegionalSentimentManifestStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalSentimentManifestMovesToCompleted()
    {
        var item = RegionalSentimentManifest.NewRegionalSentimentManifest("code-64", "name-23");
        try
        {
            item.MoveTo(RegionalSentimentManifestStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalSentimentManifestStatus.Completed, "status");
    }

    public static void RegionalSentimentManifestRejectsMovesFromCancelled()
    {
        var item = RegionalSentimentManifest.NewRegionalSentimentManifest("code-64", "name-23");
        item.Status = RegionalSentimentManifestStatus.Cancelled;
        Check.Fails(() => item.MoveTo(RegionalSentimentManifestStatus.Planned), "move");
    }

    public static void RunAll()
    {
        RegionalSentimentManifestStartsInPlanned();
        RegionalSentimentManifestMovesToCompleted();
        RegionalSentimentManifestRejectsMovesFromCancelled();
    }
}
