namespace Acme.Workforce.Engagement.Tests.Sentiment.Milestone;

using Acme.Workforce.Engagement.Sentiment.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class SentimentMilestoneViewChecks
{
    public static void SentimentMilestoneViewShowsTheStatusInCapitals()
    {
        var item = SentimentMilestone.NewSentimentMilestone("code-717", "label-154");
        var view = SentimentMilestoneView.ToSentimentMilestoneView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SentimentMilestoneView.ToSentimentMilestoneViews(new List<SentimentMilestone> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SentimentMilestoneViewShowsTheStatusInCapitals();
    }
}
