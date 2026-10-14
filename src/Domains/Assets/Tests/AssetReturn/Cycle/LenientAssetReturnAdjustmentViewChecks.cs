namespace Acme.Workforce.Assets.Tests.AssetReturn.Cycle;

using Acme.Workforce.Assets.AssetReturn.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class LenientAssetReturnAdjustmentViewChecks
{
    public static void LenientAssetReturnAdjustmentViewShowsTheStatusInCapitals()
    {
        var item = LenientAssetReturnAdjustment.NewLenientAssetReturnAdjustment("code-268", "title-245");
        var view = LenientAssetReturnAdjustmentView.ToLenientAssetReturnAdjustmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LenientAssetReturnAdjustmentView.ToLenientAssetReturnAdjustmentViews(new List<LenientAssetReturnAdjustment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LenientAssetReturnAdjustmentViewShowsTheStatusInCapitals();
    }
}
