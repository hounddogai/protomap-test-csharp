namespace Acme.Workforce.Assets.Tests.Laptop.Review;

using Acme.Workforce.Assets.Laptop.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class CappedLaptopManifestViewChecks
{
    public static void CappedLaptopManifestViewShowsTheStatusInCapitals()
    {
        var item = CappedLaptopManifest.NewCappedLaptopManifest("code-621", "name-317");
        var view = CappedLaptopManifestView.ToCappedLaptopManifestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CappedLaptopManifestView.ToCappedLaptopManifestViews(new List<CappedLaptopManifest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CappedLaptopManifestViewShowsTheStatusInCapitals();
    }
}
