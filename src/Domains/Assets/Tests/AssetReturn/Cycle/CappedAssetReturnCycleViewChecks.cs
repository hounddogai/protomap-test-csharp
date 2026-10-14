namespace Acme.Workforce.Assets.Tests.AssetReturn.Cycle;

using Acme.Workforce.Assets.AssetReturn.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class CappedAssetReturnCycleViewChecks
{
    public static void CappedAssetReturnCycleViewShowsTheStatusInCapitals()
    {
        var item = CappedAssetReturnCycle.NewCappedAssetReturnCycle("code-567", "name-357");
        var view = CappedAssetReturnCycleView.ToCappedAssetReturnCycleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CappedAssetReturnCycleView.ToCappedAssetReturnCycleViews(new List<CappedAssetReturnCycle> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CappedAssetReturnCycleViewShowsTheStatusInCapitals();
    }
}
