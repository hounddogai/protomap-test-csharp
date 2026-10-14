namespace Acme.Workforce.Engagement.Tests.Sentiment.Snapshot;

using Acme.Workforce.Engagement.Sentiment.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class SentimentMetricViewChecks
{
    public static void SentimentMetricViewShowsTheStatusInCapitals()
    {
        var item = SentimentMetric.NewSentimentMetric("code-774", "name-977");
        var view = SentimentMetricView.ToSentimentMetricView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SentimentMetricView.ToSentimentMetricViews(new List<SentimentMetric> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SentimentMetricViewShowsTheStatusInCapitals();
    }
}
