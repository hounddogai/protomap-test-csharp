namespace Acme.Workforce.Assets.Tests.AssetReturn.Adjustment;

using Acme.Workforce.Assets.AssetReturn.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class AssetReturnAdjustmentChecks
{
    public static void AssetReturnAdjustmentStartsInNew()
    {
        var item = AssetReturnAdjustment.NewAssetReturnAdjustment("code-775", "title-240");
        Check.Equal(item.Status, AssetReturnAdjustmentStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AssetReturnAdjustmentMovesToDone()
    {
        var item = AssetReturnAdjustment.NewAssetReturnAdjustment("code-775", "title-240");
        try
        {
            item.MoveTo(AssetReturnAdjustmentStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AssetReturnAdjustmentStatus.Done, "status");
    }

    public static void AssetReturnAdjustmentRejectsMovesFromFailed()
    {
        var item = AssetReturnAdjustment.NewAssetReturnAdjustment("code-775", "title-240");
        item.Status = AssetReturnAdjustmentStatus.Failed;
        Check.Fails(() => item.MoveTo(AssetReturnAdjustmentStatus.New), "move");
    }

    public static void RunAll()
    {
        AssetReturnAdjustmentStartsInNew();
        AssetReturnAdjustmentMovesToDone();
        AssetReturnAdjustmentRejectsMovesFromFailed();
    }
}
