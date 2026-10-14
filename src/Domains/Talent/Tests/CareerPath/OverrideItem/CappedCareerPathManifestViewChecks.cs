namespace Acme.Workforce.Talent.Tests.CareerPath.OverrideItem;

using Acme.Workforce.Talent.CareerPath.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class CappedCareerPathManifestViewChecks
{
    public static void CappedCareerPathManifestViewShowsTheStatusInCapitals()
    {
        var item = CappedCareerPathManifest.NewCappedCareerPathManifest("code-324", "name-575");
        var view = CappedCareerPathManifestView.ToCappedCareerPathManifestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CappedCareerPathManifestView.ToCappedCareerPathManifestViews(new List<CappedCareerPathManifest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CappedCareerPathManifestViewShowsTheStatusInCapitals();
    }
}
