namespace Acme.Workforce.Engagement.Tests.Feedback.Manifest;

using Acme.Workforce.Engagement.Feedback.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class FeedbackManifestViewChecks
{
    public static void FeedbackManifestViewShowsTheStatusInCapitals()
    {
        var item = FeedbackManifest.NewFeedbackManifest("code-111", "name-730");
        var view = FeedbackManifestView.ToFeedbackManifestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FeedbackManifestView.ToFeedbackManifestViews(new List<FeedbackManifest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FeedbackManifestViewShowsTheStatusInCapitals();
    }
}
