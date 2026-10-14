namespace Acme.Workforce.Engagement.Tests.Feedback.Group;

using Acme.Workforce.Engagement.Feedback.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class FeedbackGroupViewChecks
{
    public static void FeedbackGroupViewShowsTheStatusInCapitals()
    {
        var item = FeedbackGroup.NewFeedbackGroup("code-862", "name-565");
        var view = FeedbackGroupView.ToFeedbackGroupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FeedbackGroupView.ToFeedbackGroupViews(new List<FeedbackGroup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FeedbackGroupViewShowsTheStatusInCapitals();
    }
}
