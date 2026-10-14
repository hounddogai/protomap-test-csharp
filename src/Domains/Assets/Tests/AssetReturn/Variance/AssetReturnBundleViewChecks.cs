namespace Acme.Workforce.Assets.Tests.AssetReturn.Variance;

using Acme.Workforce.Assets.AssetReturn.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class AssetReturnBundleViewChecks
{
    public static void AssetReturnBundleViewShowsTheStatusInCapitals()
    {
        var item = AssetReturnBundle.NewAssetReturnBundle("code-188", "name-628");
        var view = AssetReturnBundleView.ToAssetReturnBundleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AssetReturnBundleView.ToAssetReturnBundleViews(new List<AssetReturnBundle> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AssetReturnBundleViewShowsTheStatusInCapitals();
    }
}
