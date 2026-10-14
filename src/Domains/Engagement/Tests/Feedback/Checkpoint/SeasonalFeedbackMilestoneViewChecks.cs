namespace Acme.Workforce.Engagement.Tests.Feedback.Checkpoint;

using Acme.Workforce.Engagement.Feedback.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class SeasonalFeedbackMilestoneViewChecks
{
    public static void SeasonalFeedbackMilestoneViewShowsTheStatusInCapitals()
    {
        var item = SeasonalFeedbackMilestone.NewSeasonalFeedbackMilestone("code-620", "name-474");
        var view = SeasonalFeedbackMilestoneView.ToSeasonalFeedbackMilestoneView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SeasonalFeedbackMilestoneView.ToSeasonalFeedbackMilestoneViews(new List<SeasonalFeedbackMilestone> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SeasonalFeedbackMilestoneViewShowsTheStatusInCapitals();
    }
}
