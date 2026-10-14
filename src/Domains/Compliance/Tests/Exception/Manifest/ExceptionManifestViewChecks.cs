namespace Acme.Workforce.Compliance.Tests.Exception.Manifest;

using Acme.Workforce.Compliance.Exception.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class ExceptionManifestViewChecks
{
    public static void ExceptionManifestViewShowsTheStatusInCapitals()
    {
        var item = ExceptionManifest.NewExceptionManifest("code-333", "title-102");
        var view = ExceptionManifestView.ToExceptionManifestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ExceptionManifestView.ToExceptionManifestViews(new List<ExceptionManifest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ExceptionManifestViewShowsTheStatusInCapitals();
    }
}
