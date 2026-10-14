namespace Acme.Workforce.Engagement.Tests.Feedback.Notice;

using Acme.Workforce.Engagement.Feedback.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class TieredFeedbackPreferenceViewChecks
{
    public static void TieredFeedbackPreferenceViewShowsTheStatusInCapitals()
    {
        var item = TieredFeedbackPreference.NewTieredFeedbackPreference("code-430", "name-420");
        var view = TieredFeedbackPreferenceView.ToTieredFeedbackPreferenceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TieredFeedbackPreferenceView.ToTieredFeedbackPreferenceViews(new List<TieredFeedbackPreference> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TieredFeedbackPreferenceViewShowsTheStatusInCapitals();
    }
}
