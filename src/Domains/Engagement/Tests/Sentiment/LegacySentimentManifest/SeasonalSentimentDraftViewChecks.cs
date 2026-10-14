namespace Acme.Workforce.Engagement.Tests.Sentiment.LegacySentimentManifest;

using Acme.Workforce.Engagement.Sentiment.LegacySentimentManifest;

/// <summary>Checks of legacy sentiment manifest code, run by the project's runner.</summary>
public static class SeasonalSentimentDraftViewChecks
{
    public static void SeasonalSentimentDraftViewShowsTheStatusInCapitals()
    {
        var item = SeasonalSentimentDraft.NewSeasonalSentimentDraft("code-747", "label-953");
        var view = SeasonalSentimentDraftView.ToSeasonalSentimentDraftView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SeasonalSentimentDraftView.ToSeasonalSentimentDraftViews(new List<SeasonalSentimentDraft> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SeasonalSentimentDraftViewShowsTheStatusInCapitals();
    }
}
