namespace Acme.Workforce.Engagement.Tests.Recognition.Milestone;

using Acme.Workforce.Engagement.Recognition.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class ScheduledRecognitionBundleViewChecks
{
    public static void ScheduledRecognitionBundleViewShowsTheStatusInCapitals()
    {
        var item = ScheduledRecognitionBundle.NewScheduledRecognitionBundle("code-319", "label-276");
        var view = ScheduledRecognitionBundleView.ToScheduledRecognitionBundleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledRecognitionBundleView.ToScheduledRecognitionBundleViews(new List<ScheduledRecognitionBundle> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledRecognitionBundleViewShowsTheStatusInCapitals();
    }
}
