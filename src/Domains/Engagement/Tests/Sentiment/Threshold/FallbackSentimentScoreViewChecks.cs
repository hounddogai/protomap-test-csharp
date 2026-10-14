namespace Acme.Workforce.Engagement.Tests.Sentiment.Threshold;

using Acme.Workforce.Engagement.Sentiment.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class FallbackSentimentScoreViewChecks
{
    public static void FallbackSentimentScoreViewShowsTheStatusInCapitals()
    {
        var item = FallbackSentimentScore.NewFallbackSentimentScore("code-978", "label-461");
        var view = FallbackSentimentScoreView.ToFallbackSentimentScoreView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FallbackSentimentScoreView.ToFallbackSentimentScoreViews(new List<FallbackSentimentScore> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FallbackSentimentScoreViewShowsTheStatusInCapitals();
    }
}
