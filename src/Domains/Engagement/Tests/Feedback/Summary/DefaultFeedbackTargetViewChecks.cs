namespace Acme.Workforce.Engagement.Tests.Feedback.Summary;

using Acme.Workforce.Engagement.Feedback.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class DefaultFeedbackTargetViewChecks
{
    public static void DefaultFeedbackTargetViewShowsTheStatusInCapitals()
    {
        var item = DefaultFeedbackTarget.NewDefaultFeedbackTarget("code-712", "title-483");
        var view = DefaultFeedbackTargetView.ToDefaultFeedbackTargetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DefaultFeedbackTargetView.ToDefaultFeedbackTargetViews(new List<DefaultFeedbackTarget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DefaultFeedbackTargetViewShowsTheStatusInCapitals();
    }
}
