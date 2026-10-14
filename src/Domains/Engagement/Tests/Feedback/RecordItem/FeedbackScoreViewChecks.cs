namespace Acme.Workforce.Engagement.Tests.Feedback.RecordItem;

using Acme.Workforce.Engagement.Feedback.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class FeedbackScoreViewChecks
{
    public static void FeedbackScoreViewShowsTheStatusInCapitals()
    {
        var item = FeedbackScore.NewFeedbackScore("code-36", "name-110");
        var view = FeedbackScoreView.ToFeedbackScoreView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FeedbackScoreView.ToFeedbackScoreViews(new List<FeedbackScore> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FeedbackScoreViewShowsTheStatusInCapitals();
    }
}
