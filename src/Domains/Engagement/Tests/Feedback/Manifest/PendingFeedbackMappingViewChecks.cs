namespace Acme.Workforce.Engagement.Tests.Feedback.Manifest;

using Acme.Workforce.Engagement.Feedback.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class PendingFeedbackMappingViewChecks
{
    public static void PendingFeedbackMappingViewShowsTheStatusInCapitals()
    {
        var item = PendingFeedbackMapping.NewPendingFeedbackMapping("code-571", "name-791");
        var view = PendingFeedbackMappingView.ToPendingFeedbackMappingView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PendingFeedbackMappingView.ToPendingFeedbackMappingViews(new List<PendingFeedbackMapping> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PendingFeedbackMappingViewShowsTheStatusInCapitals();
    }
}
