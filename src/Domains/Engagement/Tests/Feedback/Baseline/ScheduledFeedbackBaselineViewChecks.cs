namespace Acme.Workforce.Engagement.Tests.Feedback.Baseline;

using Acme.Workforce.Engagement.Feedback.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class ScheduledFeedbackBaselineViewChecks
{
    public static void ScheduledFeedbackBaselineViewShowsTheStatusInCapitals()
    {
        var item = ScheduledFeedbackBaseline.NewScheduledFeedbackBaseline("code-68", "label-810");
        var view = ScheduledFeedbackBaselineView.ToScheduledFeedbackBaselineView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledFeedbackBaselineView.ToScheduledFeedbackBaselineViews(new List<ScheduledFeedbackBaseline> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledFeedbackBaselineViewShowsTheStatusInCapitals();
    }
}
