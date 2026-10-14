namespace Acme.Workforce.Engagement.Tests.Feedback.FeedbackScore;

using Acme.Workforce.Engagement.Feedback.FeedbackScore;

/// <summary>Checks of feedback score code, run by the project's runner.</summary>
public static class SeasonalFeedbackScoreViewChecks
{
    public static void SeasonalFeedbackScoreViewShowsTheStatusInCapitals()
    {
        var item = SeasonalFeedbackScore.NewSeasonalFeedbackScore("code-214", "title-530");
        var view = SeasonalFeedbackScoreView.ToSeasonalFeedbackScoreView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SeasonalFeedbackScoreView.ToSeasonalFeedbackScoreViews(new List<SeasonalFeedbackScore> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SeasonalFeedbackScoreViewShowsTheStatusInCapitals();
    }
}
