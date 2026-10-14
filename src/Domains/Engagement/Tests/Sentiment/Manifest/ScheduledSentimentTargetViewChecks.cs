namespace Acme.Workforce.Engagement.Tests.Sentiment.Manifest;

using Acme.Workforce.Engagement.Sentiment.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class ScheduledSentimentTargetViewChecks
{
    public static void ScheduledSentimentTargetViewShowsTheStatusInCapitals()
    {
        var item = ScheduledSentimentTarget.NewScheduledSentimentTarget("code-371", "name-460");
        var view = ScheduledSentimentTargetView.ToScheduledSentimentTargetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledSentimentTargetView.ToScheduledSentimentTargetViews(new List<ScheduledSentimentTarget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledSentimentTargetViewShowsTheStatusInCapitals();
    }
}
