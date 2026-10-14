namespace Acme.Workforce.Engagement.Tests.Feedback.SeasonalFeedbackReservation;

using Acme.Workforce.Engagement.Feedback.SeasonalFeedbackReservation;

/// <summary>Checks of seasonal feedback reservation code, run by the project's runner.</summary>
public static class RollingFeedbackScoreViewChecks
{
    public static void RollingFeedbackScoreViewShowsTheStatusInCapitals()
    {
        var item = RollingFeedbackScore.NewRollingFeedbackScore("code-825", "name-669");
        var view = RollingFeedbackScoreView.ToRollingFeedbackScoreView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RollingFeedbackScoreView.ToRollingFeedbackScoreViews(new List<RollingFeedbackScore> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RollingFeedbackScoreViewShowsTheStatusInCapitals();
    }
}
