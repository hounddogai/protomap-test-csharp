namespace Acme.Workforce.Assets.Tests.AssetReturn.Batch;

using Acme.Workforce.Assets.AssetReturn.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class PendingAssetReturnAdjustmentChecks
{
    public static void PendingAssetReturnAdjustmentStartsInNew()
    {
        var item = PendingAssetReturnAdjustment.NewPendingAssetReturnAdjustment("code-331", "title-474");
        Check.Equal(item.Status, PendingAssetReturnAdjustmentStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingAssetReturnAdjustmentMovesToRunning()
    {
        var item = PendingAssetReturnAdjustment.NewPendingAssetReturnAdjustment("code-331", "title-474");
        try
        {
            item.MoveTo(PendingAssetReturnAdjustmentStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingAssetReturnAdjustmentStatus.Running, "status");
    }

    public static void PendingAssetReturnAdjustmentRejectsMovesFromFailed()
    {
        var item = PendingAssetReturnAdjustment.NewPendingAssetReturnAdjustment("code-331", "title-474");
        item.Status = PendingAssetReturnAdjustmentStatus.Failed;
        Check.Fails(() => item.MoveTo(PendingAssetReturnAdjustmentStatus.New), "move");
    }

    public static void RunAll()
    {
        PendingAssetReturnAdjustmentStartsInNew();
        PendingAssetReturnAdjustmentMovesToRunning();
        PendingAssetReturnAdjustmentRejectsMovesFromFailed();
    }
}
