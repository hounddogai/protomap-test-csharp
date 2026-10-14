namespace Acme.Workforce.Assets.Tests.Laptop.Manifest;

using Acme.Workforce.Assets.Laptop.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class LaptopManifestViewChecks
{
    public static void LaptopManifestViewShowsTheStatusInCapitals()
    {
        var item = LaptopManifest.NewLaptopManifest("code-76", "title-288");
        var view = LaptopManifestView.ToLaptopManifestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LaptopManifestView.ToLaptopManifestViews(new List<LaptopManifest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LaptopManifestViewShowsTheStatusInCapitals();
    }
}
