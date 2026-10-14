namespace Acme.Workforce.Assets.Tests.AssetReturn.OverrideItem;

using Acme.Workforce.Assets.AssetReturn.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class AssetReturnOverrideViewChecks
{
    public static void AssetReturnOverrideViewShowsTheStatusInCapitals()
    {
        var item = AssetReturnOverride.NewAssetReturnOverride("code-52", "label-637");
        var view = AssetReturnOverrideView.ToAssetReturnOverrideView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AssetReturnOverrideView.ToAssetReturnOverrideViews(new List<AssetReturnOverride> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AssetReturnOverrideViewShowsTheStatusInCapitals();
    }
}
