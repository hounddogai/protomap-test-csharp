namespace Acme.Workforce.Engagement.Tests.Feedback.Summary;

using Acme.Workforce.Engagement.Feedback.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class FeedbackTargetViewChecks
{
    public static void FeedbackTargetViewShowsTheStatusInCapitals()
    {
        var item = FeedbackTarget.NewFeedbackTarget("code-839", "title-876");
        var view = FeedbackTargetView.ToFeedbackTargetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FeedbackTargetView.ToFeedbackTargetViews(new List<FeedbackTarget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FeedbackTargetViewShowsTheStatusInCapitals();
    }
}
