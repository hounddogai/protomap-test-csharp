namespace Acme.Workforce.Engagement.Tests.Feedback.Snapshot;

using Acme.Workforce.Engagement.Feedback.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class FeedbackAllocationViewChecks
{
    public static void FeedbackAllocationViewShowsTheStatusInCapitals()
    {
        var item = FeedbackAllocation.NewFeedbackAllocation("code-996", "title-266");
        var view = FeedbackAllocationView.ToFeedbackAllocationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FeedbackAllocationView.ToFeedbackAllocationViews(new List<FeedbackAllocation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FeedbackAllocationViewShowsTheStatusInCapitals();
    }
}
