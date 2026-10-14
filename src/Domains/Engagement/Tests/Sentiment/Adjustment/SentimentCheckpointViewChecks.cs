namespace Acme.Workforce.Engagement.Tests.Sentiment.Adjustment;

using Acme.Workforce.Engagement.Sentiment.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class SentimentCheckpointViewChecks
{
    public static void SentimentCheckpointViewShowsTheStatusInCapitals()
    {
        var item = SentimentCheckpoint.NewSentimentCheckpoint("code-751", "name-633");
        var view = SentimentCheckpointView.ToSentimentCheckpointView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SentimentCheckpointView.ToSentimentCheckpointViews(new List<SentimentCheckpoint> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SentimentCheckpointViewShowsTheStatusInCapitals();
    }
}
