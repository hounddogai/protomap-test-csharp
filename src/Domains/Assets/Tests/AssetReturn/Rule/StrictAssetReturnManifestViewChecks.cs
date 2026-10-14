namespace Acme.Workforce.Assets.Tests.AssetReturn.Rule;

using Acme.Workforce.Assets.AssetReturn.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class StrictAssetReturnManifestViewChecks
{
    public static void StrictAssetReturnManifestViewShowsTheStatusInCapitals()
    {
        var item = StrictAssetReturnManifest.NewStrictAssetReturnManifest("code-173", "name-877");
        var view = StrictAssetReturnManifestView.ToStrictAssetReturnManifestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StrictAssetReturnManifestView.ToStrictAssetReturnManifestViews(new List<StrictAssetReturnManifest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StrictAssetReturnManifestViewShowsTheStatusInCapitals();
    }
}
