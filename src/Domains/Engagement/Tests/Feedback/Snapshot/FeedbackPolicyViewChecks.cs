namespace Acme.Workforce.Engagement.Tests.Feedback.Snapshot;

using Acme.Workforce.Engagement.Feedback.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class FeedbackPolicyViewChecks
{
    public static void FeedbackPolicyViewShowsTheStatusInCapitals()
    {
        var item = FeedbackPolicy.NewFeedbackPolicy("code-489", "title-886");
        var view = FeedbackPolicyView.ToFeedbackPolicyView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FeedbackPolicyView.ToFeedbackPolicyViews(new List<FeedbackPolicy> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FeedbackPolicyViewShowsTheStatusInCapitals();
    }
}
