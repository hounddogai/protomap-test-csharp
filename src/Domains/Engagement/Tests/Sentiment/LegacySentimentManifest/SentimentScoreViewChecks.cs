namespace Acme.Workforce.Engagement.Tests.Sentiment.LegacySentimentManifest;

using Acme.Workforce.Engagement.Sentiment.LegacySentimentManifest;

/// <summary>Checks of legacy sentiment manifest code, run by the project's runner.</summary>
public static class SentimentScoreViewChecks
{
    public static void SentimentScoreViewShowsTheStatusInCapitals()
    {
        var item = SentimentScore.NewSentimentScore("code-740", "name-118");
        var view = SentimentScoreView.ToSentimentScoreView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SentimentScoreView.ToSentimentScoreViews(new List<SentimentScore> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SentimentScoreViewShowsTheStatusInCapitals();
    }
}
