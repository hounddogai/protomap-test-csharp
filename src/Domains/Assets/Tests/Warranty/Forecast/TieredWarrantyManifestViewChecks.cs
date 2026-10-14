namespace Acme.Workforce.Assets.Tests.Warranty.Forecast;

using Acme.Workforce.Assets.Warranty.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class TieredWarrantyManifestViewChecks
{
    public static void TieredWarrantyManifestViewShowsTheStatusInCapitals()
    {
        var item = TieredWarrantyManifest.NewTieredWarrantyManifest("code-849", "name-234");
        var view = TieredWarrantyManifestView.ToTieredWarrantyManifestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TieredWarrantyManifestView.ToTieredWarrantyManifestViews(new List<TieredWarrantyManifest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TieredWarrantyManifestViewShowsTheStatusInCapitals();
    }
}
