namespace Acme.Workforce.Engagement.Tests.Feedback.Mapping;

using Acme.Workforce.Engagement.Feedback.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class PendingFeedbackMilestoneViewChecks
{
    public static void PendingFeedbackMilestoneViewShowsTheStatusInCapitals()
    {
        var item = PendingFeedbackMilestone.NewPendingFeedbackMilestone("code-51", "name-723");
        var view = PendingFeedbackMilestoneView.ToPendingFeedbackMilestoneView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PendingFeedbackMilestoneView.ToPendingFeedbackMilestoneViews(new List<PendingFeedbackMilestone> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PendingFeedbackMilestoneViewShowsTheStatusInCapitals();
    }
}
