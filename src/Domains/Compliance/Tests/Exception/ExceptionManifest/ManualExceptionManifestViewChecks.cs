namespace Acme.Workforce.Compliance.Tests.Exception.ExceptionManifest;

using Acme.Workforce.Compliance.Exception.ExceptionManifest;

/// <summary>Checks of exception manifest code, run by the project's runner.</summary>
public static class ManualExceptionManifestViewChecks
{
    public static void ManualExceptionManifestViewShowsTheStatusInCapitals()
    {
        var item = ManualExceptionManifest.NewManualExceptionManifest("code-741", "name-390");
        var view = ManualExceptionManifestView.ToManualExceptionManifestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ManualExceptionManifestView.ToManualExceptionManifestViews(new List<ManualExceptionManifest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ManualExceptionManifestViewShowsTheStatusInCapitals();
    }
}
