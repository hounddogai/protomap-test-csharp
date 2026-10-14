namespace Acme.Workforce.Assets.Tests.Warranty.Quota;

using Acme.Workforce.Assets.Warranty.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class StrictWarrantyManifestViewChecks
{
    public static void StrictWarrantyManifestViewShowsTheStatusInCapitals()
    {
        var item = StrictWarrantyManifest.NewStrictWarrantyManifest("code-367", "title-226");
        var view = StrictWarrantyManifestView.ToStrictWarrantyManifestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StrictWarrantyManifestView.ToStrictWarrantyManifestViews(new List<StrictWarrantyManifest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StrictWarrantyManifestViewShowsTheStatusInCapitals();
    }
}
