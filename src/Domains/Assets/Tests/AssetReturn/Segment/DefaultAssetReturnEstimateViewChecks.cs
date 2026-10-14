namespace Acme.Workforce.Assets.Tests.AssetReturn.Segment;

using Acme.Workforce.Assets.AssetReturn.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class DefaultAssetReturnEstimateViewChecks
{
    public static void DefaultAssetReturnEstimateViewShowsTheStatusInCapitals()
    {
        var item = DefaultAssetReturnEstimate.NewDefaultAssetReturnEstimate("code-600", "title-992");
        var view = DefaultAssetReturnEstimateView.ToDefaultAssetReturnEstimateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DefaultAssetReturnEstimateView.ToDefaultAssetReturnEstimateViews(new List<DefaultAssetReturnEstimate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DefaultAssetReturnEstimateViewShowsTheStatusInCapitals();
    }
}
