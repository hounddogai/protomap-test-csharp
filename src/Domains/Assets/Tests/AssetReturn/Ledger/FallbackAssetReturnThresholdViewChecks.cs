namespace Acme.Workforce.Assets.Tests.AssetReturn.Ledger;

using Acme.Workforce.Assets.AssetReturn.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class FallbackAssetReturnThresholdViewChecks
{
    public static void FallbackAssetReturnThresholdViewShowsTheStatusInCapitals()
    {
        var item = FallbackAssetReturnThreshold.NewFallbackAssetReturnThreshold("code-354", "label-466");
        var view = FallbackAssetReturnThresholdView.ToFallbackAssetReturnThresholdView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FallbackAssetReturnThresholdView.ToFallbackAssetReturnThresholdViews(new List<FallbackAssetReturnThreshold> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FallbackAssetReturnThresholdViewShowsTheStatusInCapitals();
    }
}
