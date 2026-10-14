namespace Acme.Workforce.Engagement.Tests.Sentiment.LegacySentimentManifest;

using Acme.Workforce.Engagement.Sentiment.LegacySentimentManifest;

/// <summary>Checks of legacy sentiment manifest code, run by the project's runner.</summary>
public static class SeasonalSentimentDraftChecks
{
    public static void SeasonalSentimentDraftStartsInPlanned()
    {
        var item = SeasonalSentimentDraft.NewSeasonalSentimentDraft("code-747", "label-953");
        Check.Equal(item.Status, SeasonalSentimentDraftStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalSentimentDraftMovesToCancelled()
    {
        var item = SeasonalSentimentDraft.NewSeasonalSentimentDraft("code-747", "label-953");
        try
        {
            item.MoveTo(SeasonalSentimentDraftStatus.Cancelled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalSentimentDraftStatus.Cancelled, "status");
    }

    public static void SeasonalSentimentDraftRejectsMovesFromCancelled()
    {
        var item = SeasonalSentimentDraft.NewSeasonalSentimentDraft("code-747", "label-953");
        item.Status = SeasonalSentimentDraftStatus.Cancelled;
        Check.Fails(() => item.MoveTo(SeasonalSentimentDraftStatus.Planned), "move");
    }

    public static void RunAll()
    {
        SeasonalSentimentDraftStartsInPlanned();
        SeasonalSentimentDraftMovesToCancelled();
        SeasonalSentimentDraftRejectsMovesFromCancelled();
    }
}
