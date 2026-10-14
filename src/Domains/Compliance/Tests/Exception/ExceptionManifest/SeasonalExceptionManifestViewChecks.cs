namespace Acme.Workforce.Compliance.Tests.Exception.ExceptionManifest;

using Acme.Workforce.Compliance.Exception.ExceptionManifest;

/// <summary>Checks of exception manifest code, run by the project's runner.</summary>
public static class SeasonalExceptionManifestViewChecks
{
    public static void SeasonalExceptionManifestViewShowsTheStatusInCapitals()
    {
        var item = SeasonalExceptionManifest.NewSeasonalExceptionManifest("code-35", "label-841");
        var view = SeasonalExceptionManifestView.ToSeasonalExceptionManifestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SeasonalExceptionManifestView.ToSeasonalExceptionManifestViews(new List<SeasonalExceptionManifest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SeasonalExceptionManifestViewShowsTheStatusInCapitals();
    }
}
