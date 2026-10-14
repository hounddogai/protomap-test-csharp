namespace Acme.Workforce.Assets.Tests.Refresh.Batch;

using Acme.Workforce.Assets.Refresh.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class RefreshBatchChecks
{
    public static void RefreshBatchStartsInRequested()
    {
        var item = RefreshBatch.NewRefreshBatch("code-366", "label-821");
        Check.Equal(item.Status, RefreshBatchStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RefreshBatchMovesToGranted()
    {
        var item = RefreshBatch.NewRefreshBatch("code-366", "label-821");
        try
        {
            item.MoveTo(RefreshBatchStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RefreshBatchStatus.Granted, "status");
    }

    public static void RefreshBatchRejectsMovesFromExpired()
    {
        var item = RefreshBatch.NewRefreshBatch("code-366", "label-821");
        item.Status = RefreshBatchStatus.Expired;
        Check.Fails(() => item.MoveTo(RefreshBatchStatus.Requested), "move");
    }

    public static void RunAll()
    {
        RefreshBatchStartsInRequested();
        RefreshBatchMovesToGranted();
        RefreshBatchRejectsMovesFromExpired();
    }
}
