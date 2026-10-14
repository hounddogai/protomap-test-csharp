namespace Acme.Workforce.Assets.Tests.AssetReturn.Template;

using Acme.Workforce.Assets.AssetReturn.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class ScheduledAssetReturnBatchChecks
{
    public static void ScheduledAssetReturnBatchStartsInDraft()
    {
        var item = ScheduledAssetReturnBatch.NewScheduledAssetReturnBatch("code-601", "name-193");
        Check.Equal(item.Status, ScheduledAssetReturnBatchStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledAssetReturnBatchMovesToPending()
    {
        var item = ScheduledAssetReturnBatch.NewScheduledAssetReturnBatch("code-601", "name-193");
        try
        {
            item.MoveTo(ScheduledAssetReturnBatchStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledAssetReturnBatchStatus.Pending, "status");
    }

    public static void ScheduledAssetReturnBatchRejectsMovesFromClosed()
    {
        var item = ScheduledAssetReturnBatch.NewScheduledAssetReturnBatch("code-601", "name-193");
        item.Status = ScheduledAssetReturnBatchStatus.Closed;
        Check.Fails(() => item.MoveTo(ScheduledAssetReturnBatchStatus.Draft), "move");
    }

    public static void RunAll()
    {
        ScheduledAssetReturnBatchStartsInDraft();
        ScheduledAssetReturnBatchMovesToPending();
        ScheduledAssetReturnBatchRejectsMovesFromClosed();
    }
}
