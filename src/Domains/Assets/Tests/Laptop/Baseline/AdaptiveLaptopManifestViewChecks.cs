namespace Acme.Workforce.Assets.Tests.Laptop.Baseline;

using Acme.Workforce.Assets.Laptop.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class AdaptiveLaptopManifestViewChecks
{
    public static void AdaptiveLaptopManifestViewShowsTheStatusInCapitals()
    {
        var item = AdaptiveLaptopManifest.NewAdaptiveLaptopManifest("code-914", "title-349");
        var view = AdaptiveLaptopManifestView.ToAdaptiveLaptopManifestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdaptiveLaptopManifestView.ToAdaptiveLaptopManifestViews(new List<AdaptiveLaptopManifest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdaptiveLaptopManifestViewShowsTheStatusInCapitals();
    }
}
