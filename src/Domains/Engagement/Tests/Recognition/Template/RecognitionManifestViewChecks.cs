namespace Acme.Workforce.Engagement.Tests.Recognition.Template;

using Acme.Workforce.Engagement.Recognition.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class RecognitionManifestViewChecks
{
    public static void RecognitionManifestViewShowsTheStatusInCapitals()
    {
        var item = RecognitionManifest.NewRecognitionManifest("code-762", "label-215");
        var view = RecognitionManifestView.ToRecognitionManifestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RecognitionManifestView.ToRecognitionManifestViews(new List<RecognitionManifest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RecognitionManifestViewShowsTheStatusInCapitals();
    }
}
