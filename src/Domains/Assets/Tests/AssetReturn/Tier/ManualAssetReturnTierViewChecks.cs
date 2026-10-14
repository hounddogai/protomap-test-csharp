namespace Acme.Workforce.Assets.Tests.AssetReturn.Tier;

using Acme.Workforce.Assets.AssetReturn.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class ManualAssetReturnTierViewChecks
{
    public static void ManualAssetReturnTierViewShowsTheStatusInCapitals()
    {
        var item = ManualAssetReturnTier.NewManualAssetReturnTier("code-500", "title-806");
        var view = ManualAssetReturnTierView.ToManualAssetReturnTierView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ManualAssetReturnTierView.ToManualAssetReturnTierViews(new List<ManualAssetReturnTier> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ManualAssetReturnTierViewShowsTheStatusInCapitals();
    }
}
