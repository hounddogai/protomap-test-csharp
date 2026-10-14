namespace Acme.Workforce.Assets.Tests.AssetReturn.Batch;

using Acme.Workforce.Assets.AssetReturn.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class ManualAssetReturnBatchChecks
{
    public static void ManualAssetReturnBatchStartsInProposed()
    {
        var item = ManualAssetReturnBatch.NewManualAssetReturnBatch("code-959", "label-370");
        Check.Equal(item.Status, ManualAssetReturnBatchStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualAssetReturnBatchMovesToApproved()
    {
        var item = ManualAssetReturnBatch.NewManualAssetReturnBatch("code-959", "label-370");
        try
        {
            item.MoveTo(ManualAssetReturnBatchStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualAssetReturnBatchStatus.Approved, "status");
    }

    public static void ManualAssetReturnBatchRejectsMovesFromWithdrawn()
    {
        var item = ManualAssetReturnBatch.NewManualAssetReturnBatch("code-959", "label-370");
        item.Status = ManualAssetReturnBatchStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(ManualAssetReturnBatchStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        ManualAssetReturnBatchStartsInProposed();
        ManualAssetReturnBatchMovesToApproved();
        ManualAssetReturnBatchRejectsMovesFromWithdrawn();
    }
}
