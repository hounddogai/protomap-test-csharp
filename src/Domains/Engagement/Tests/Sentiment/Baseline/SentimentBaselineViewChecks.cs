namespace Acme.Workforce.Engagement.Tests.Sentiment.Baseline;

using Acme.Workforce.Engagement.Sentiment.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class SentimentBaselineViewChecks
{
    public static void SentimentBaselineViewShowsTheStatusInCapitals()
    {
        var item = SentimentBaseline.NewSentimentBaseline("code-408", "label-691");
        var view = SentimentBaselineView.ToSentimentBaselineView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SentimentBaselineView.ToSentimentBaselineViews(new List<SentimentBaseline> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SentimentBaselineViewShowsTheStatusInCapitals();
    }
}
