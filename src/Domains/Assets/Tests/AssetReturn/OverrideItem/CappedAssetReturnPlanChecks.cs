namespace Acme.Workforce.Assets.Tests.AssetReturn.OverrideItem;

using Acme.Workforce.Assets.AssetReturn.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class CappedAssetReturnPlanChecks
{
    public static void CappedAssetReturnPlanStartsInNew()
    {
        var item = CappedAssetReturnPlan.NewCappedAssetReturnPlan("code-933", "name-463");
        Check.Equal(item.Status, CappedAssetReturnPlanStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedAssetReturnPlanMovesToFailed()
    {
        var item = CappedAssetReturnPlan.NewCappedAssetReturnPlan("code-933", "name-463");
        try
        {
            item.MoveTo(CappedAssetReturnPlanStatus.Failed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedAssetReturnPlanStatus.Failed, "status");
    }

    public static void CappedAssetReturnPlanRejectsMovesFromFailed()
    {
        var item = CappedAssetReturnPlan.NewCappedAssetReturnPlan("code-933", "name-463");
        item.Status = CappedAssetReturnPlanStatus.Failed;
        Check.Fails(() => item.MoveTo(CappedAssetReturnPlanStatus.New), "move");
    }

    public static void RunAll()
    {
        CappedAssetReturnPlanStartsInNew();
        CappedAssetReturnPlanMovesToFailed();
        CappedAssetReturnPlanRejectsMovesFromFailed();
    }
}
