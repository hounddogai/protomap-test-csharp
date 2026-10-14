namespace Acme.Workforce.Engagement.Tests.Feedback.Proposal;

using Acme.Workforce.Engagement.Feedback.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class FeedbackBaselineViewChecks
{
    public static void FeedbackBaselineViewShowsTheStatusInCapitals()
    {
        var item = FeedbackBaseline.NewFeedbackBaseline("code-670", "title-820");
        var view = FeedbackBaselineView.ToFeedbackBaselineView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FeedbackBaselineView.ToFeedbackBaselineViews(new List<FeedbackBaseline> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FeedbackBaselineViewShowsTheStatusInCapitals();
    }
}
