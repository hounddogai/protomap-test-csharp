namespace Acme.Workforce.Assets.Tests.AssetReturn.Estimate;

using Acme.Workforce.Assets.AssetReturn.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class AssetReturnEstimateViewChecks
{
    public static void AssetReturnEstimateViewShowsTheStatusInCapitals()
    {
        var item = AssetReturnEstimate.NewAssetReturnEstimate("code-296", "title-603");
        var view = AssetReturnEstimateView.ToAssetReturnEstimateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AssetReturnEstimateView.ToAssetReturnEstimateViews(new List<AssetReturnEstimate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AssetReturnEstimateViewShowsTheStatusInCapitals();
    }
}
