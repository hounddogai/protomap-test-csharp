namespace Acme.Workforce.Assets.Tests.AssetReturn.Draft;

using Acme.Workforce.Assets.AssetReturn.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class AssetReturnManifestViewChecks
{
    public static void AssetReturnManifestViewShowsTheStatusInCapitals()
    {
        var item = AssetReturnManifest.NewAssetReturnManifest("code-221", "label-400");
        var view = AssetReturnManifestView.ToAssetReturnManifestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AssetReturnManifestView.ToAssetReturnManifestViews(new List<AssetReturnManifest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AssetReturnManifestViewShowsTheStatusInCapitals();
    }
}
