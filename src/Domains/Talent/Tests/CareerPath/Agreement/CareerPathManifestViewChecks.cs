namespace Acme.Workforce.Talent.Tests.CareerPath.Agreement;

using Acme.Workforce.Talent.CareerPath.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class CareerPathManifestViewChecks
{
    public static void CareerPathManifestViewShowsTheStatusInCapitals()
    {
        var item = CareerPathManifest.NewCareerPathManifest("code-597", "title-514");
        var view = CareerPathManifestView.ToCareerPathManifestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CareerPathManifestView.ToCareerPathManifestViews(new List<CareerPathManifest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CareerPathManifestViewShowsTheStatusInCapitals();
    }
}
