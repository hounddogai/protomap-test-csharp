namespace Acme.Workforce.Assets.Tests.AssetReturn.Variance;

using Acme.Workforce.Assets.AssetReturn.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class AssetReturnWindowViewChecks
{
    public static void AssetReturnWindowViewShowsTheStatusInCapitals()
    {
        var item = AssetReturnWindow.NewAssetReturnWindow("code-673", "title-120");
        var view = AssetReturnWindowView.ToAssetReturnWindowView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AssetReturnWindowView.ToAssetReturnWindowViews(new List<AssetReturnWindow> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AssetReturnWindowViewShowsTheStatusInCapitals();
    }
}
